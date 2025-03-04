using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Fluidic.Tests.Extensions;

public sealed class TestAdditionalFile(string path, SourceText text) : AdditionalText
{
    public override string Path => $"<global namespace>.{path}";

    public override SourceText GetText(CancellationToken cancellationToken)
    {
        return text;
    }
}

public static class GeneratorDriverExtensions
{
    internal static GeneratorDriver BuildDriver(
        this string? source,
        ImmutableArray<AdditionalText> additionalTexts
    )
    {
        var compilation = CSharpCompilation.Create(
            "name",
            source != null ? [CSharpSyntaxTree.ParseText(source)] : [],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StringTemplateAttribute).Assembly.Location),
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var generator = new StringTemplateSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).AddAdditionalTexts(additionalTexts);
        return driver.RunGenerators(compilation);
    }

    internal static Task VerifyStringTemplate(
        this string? source,
        [CallerFilePath] string sourceFile = ""
    )
    {
        var driver = $$"""
            using System;
            using Fluidic;

            internal static class Test
            {
               {{source}}
            }
            """.BuildDriver([]);
        return Verify(driver, sourceFile: sourceFile).IgnoreStandardSupportCode();
    }

    internal static Task VerifyFileTemplate(
        this string? source,
        ImmutableArray<AdditionalText> additionalTexts,
        [CallerFilePath] string sourceFile = ""
    )
    {
        var driver = $$"""
            using System;
            using Fluidic;

            internal static class Test
            {
               {{source}}
            }
            """.BuildDriver(additionalTexts);
        return Verify(driver, sourceFile: sourceFile).IgnoreStandardSupportCode();
    }

    internal static SettingsTask IgnoreStandardSupportCode(this SettingsTask settings)
    {
        return settings.IgnoreGeneratedResult(x =>
            x.HintName is "StringTemplateAttribute.g.cs" or "FileTemplateAttribute.g.cs"
        );
    }
}
