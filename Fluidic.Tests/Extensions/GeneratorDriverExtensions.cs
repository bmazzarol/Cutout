using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Fluidic.Tests.Extensions;

public static class GeneratorDriverExtensions
{
    internal static GeneratorDriver BuildDriver(this string? source)
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
        var driver = CSharpGeneratorDriver.Create(generator);
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
                       """.BuildDriver();
        return Verify(driver, sourceFile: sourceFile).IgnoreStandardSupportCode();
    }

    internal static SettingsTask IgnoreStandardSupportCode(this SettingsTask settings)
    {
        return settings.IgnoreGeneratedResult(x => x.HintName is "StringTemplateAttribute.g.cs");
    }
}
