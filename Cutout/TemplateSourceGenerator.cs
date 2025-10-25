using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Pasted;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cutout;

/// <summary>
/// Source generator for Cutout templates
/// </summary>
#pragma warning disable RS1038
[Generator]
#pragma warning restore RS1038
public sealed partial class TemplateSourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("TemplateAttribute.g", EmbeddedFiles.TemplateAttribute_Source);
            ctx.AddSource("FileTemplateAttribute.g", EmbeddedFiles.FileTemplateAttribute_Source);
            ctx.AddSource("RenderUtilities.g", EmbeddedFiles.RenderUtilities_Source);
        });

        // attribute-based template methods
        var attributeTemplateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Cutout.TemplateAttribute",
            IsTemplateMethod,
            BuildTemplateDetails
        );

        context.RegisterSourceOutput(attributeTemplateProvider, GenerateTemplate);

        // external config-based template methods
        var embeddedFiles = context.AdditionalTextsProvider.Select((file, _) => file).Collect();
        var configProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) => options);
        var fileTemplateAttributeProvider = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Cutout.FileTemplateAttribute",
                IsTemplateMethod,
                (x, _) => x
            )
            .Collect();

        var fileTemplateProvider = embeddedFiles
            .Combine(configProvider)
            .Combine(fileTemplateAttributeProvider)
            .SelectMany(BuildTemplateDetailsFromFile);

        context.RegisterSourceOutput(fileTemplateProvider, GenerateTemplate);
    }

    private static bool IsTemplateMethod(SyntaxNode syntax, CancellationToken token)
    {
        return syntax is MethodDeclarationSyntax mds
            // the method is either static
            && mds.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            // and returns a void
            && mds.ReturnType is PredefinedTypeSyntax pts
            && pts.Keyword.IsKind(SyntaxKind.VoidKeyword)
            // and has at least one this parameter
            && mds.ParameterList.Parameters.Any(p =>
                p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))
            );
    }

    private static IEnumerable<TemplateMethodDetails> BuildTemplateDetailsFromFile(
        (
            (ImmutableArray<AdditionalText> Files, AnalyzerConfigOptionsProvider Options) Static,
            ImmutableArray<GeneratorAttributeSyntaxContext> Methods
        ) ctx,
        CancellationToken token
    )
    {
        Dictionary<
            string,
            (
                SemanticModel semanticModel,
                MethodDeclarationSyntax methodDeclarationSyntax,
                IMethodSymbol methodSymbol
            )
        > methodLookup = new(StringComparer.Ordinal);
        foreach (var context in ctx.Methods)
        {
            if (context.TargetNode is not MethodDeclarationSyntax mds)
                continue;

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(mds, token);

            if (methodSymbol is null)
                continue;

            var ns = methodSymbol.ContainingNamespace?.ToDisplayString();
            var @class = methodSymbol.ContainingType.Name;
            var fullName = $"{ns}.{@class}.{methodSymbol.Name}";
            methodLookup.Add(fullName, (context.SemanticModel, mds, methodSymbol));
        }

        foreach (var file in ctx.Static.Files)
        {
            if (file.GetText(token)?.ToString() is not { } templateText)
            {
                continue;
            }

            var options = ctx.Static.Options.GetOptions(file);

            if (
                !options.TryGetValue("template_method", out var method)
                || string.IsNullOrWhiteSpace(method)
            )
            {
                continue;
            }

            if (!methodLookup.TryGetValue(method, out var methodContext))
            {
                continue;
            }

            var details = BuildTemplateDetailsFromSyntax(
                methodContext.semanticModel,
                methodContext.methodDeclarationSyntax,
                methodContext.methodSymbol,
                templateText
            );
            yield return details;
        }
    }

    private static TemplateMethodDetails BuildTemplateDetails(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken token
    )
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)ctx.TargetNode;

        var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(
            methodDeclarationSyntax,
            cancellationToken: token
        )!;

        return BuildTemplateDetailsFromSyntax(
            ctx.SemanticModel,
            methodDeclarationSyntax,
            methodSymbol,
            template: null
        );
    }

    private static TemplateMethodDetails BuildTemplateDetailsFromSyntax(
        SemanticModel semanticModel,
        MethodDeclarationSyntax methodDeclarationSyntax,
        IMethodSymbol methodSymbol,
        string? template
    )
    {
        var ns = methodSymbol.ContainingNamespace?.ToDisplayString();

        // get all usings
        var usings = methodDeclarationSyntax
            .TryGetUsings()
            .Add(SF.UsingDirective(SF.ParseName(" System")))
            .Add(SF.UsingDirective(SF.ParseName(" System.Diagnostics.CodeAnalysis")))
            .Add(SF.UsingDirective(SF.ParseName(" Cutout")));

        // get the method details
        var containingType = methodSymbol.ContainingType;
        var @class = containingType.Name;
        var classDetails = new ClassDetails(Name: @class, ClassSymbol: containingType);

        var name = methodDeclarationSyntax.Identifier.Text;
        var methodDetails = new MethodDetails(
            Name: name,
            MethodDeclaration: methodDeclarationSyntax,
            MethodSymbol: methodSymbol
        );

        // get the attribute details
        var attributeParts =
            template is null || string.IsNullOrWhiteSpace(template)
                ? new TemplateAttributeParts(methodDetails, semanticModel)
                : new TemplateAttributeParts(template);

        return new TemplateMethodDetails(
            Namespace: ns,
            Usings: usings,
            ClassDetails: classDetails,
            MethodDetails: methodDetails,
            AttributeDetails: attributeParts
        );
    }

    private static void GenerateTemplate(
        SourceProductionContext context,
        TemplateMethodDetails details
    )
    {
        try
        {
            _ = details.AttributeDetails.Syntaxes;
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "CUTOUT001",
                        "Template parsing error",
                        $"Failed to parse template for {details.ClassDetails.Name}.{details.MethodDetails.Name}: {ex.Message}",
                        "Cutout",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true
                    ),
                    details.MethodDetails.MethodSymbol.Locations.FirstOrDefault() ?? Location.None
                )
            );
            return;
        }

        using (var writer = new StringWriter())
        {
            var indentedWriter = new IndentedTextWriter(writer, "    ");

            WriteTemplateMethod(indentedWriter, details, includeWhitespaceReceiver: false);
            context.AddSource(
                $"{details.ClassDetails.Name}.{details.MethodDetails.Name}.g.cs",
                SourceText.From(writer.ToString(), Encoding.UTF8)
            );
        }

        using (var writer = new StringWriter())
        {
            var indentedWriter = new IndentedTextWriter(writer, "    ");

            WriteTemplateMethod(indentedWriter, details, includeWhitespaceReceiver: true);

            context.AddSource(
                $"{details.ClassDetails.Name}.{details.MethodDetails.Name}.wsr.g.cs",
                SourceText.From(writer.ToString(), Encoding.UTF8)
            );
        }
    }
}
