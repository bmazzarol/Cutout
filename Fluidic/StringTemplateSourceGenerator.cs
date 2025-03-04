using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Fluidic.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban.Parsing;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Fluidic;

[Generator]
public sealed partial class StringTemplateSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("StringTemplateAttribute.g", StringTemplateAttributeSourceCode);
            ctx.AddSource("FileTemplateAttribute.g", FileTemplateAttributeSourceCode);
        });

        var stringTemplateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Fluidic.StringTemplateAttribute",
            IsTemplateMethod,
            BuildTemplateDetails
        );

        context.RegisterSourceOutput(stringTemplateProvider, GenerateStringTemplate);

        var liquidFiles = context.AdditionalTextsProvider.Where(x =>
            x.Path.EndsWith(".liquid", StringComparison.Ordinal)
        );
        var fileTemplateProvider = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Fluidic.FileTemplateAttribute",
                IsTemplateMethod,
                BuildTemplateDetails
            )
            .Combine(liquidFiles.Collect())
            .Select(
                (x, _) =>
                {
                    var attributeDetails = BuildAttributeDetails(x.Left, x.Right);
                    return x.Left with { AttributeDetails = attributeDetails };
                }
            );

        context.RegisterSourceOutput(fileTemplateProvider, GenerateStringTemplate);
    }

    private TemplateAttributeParts BuildAttributeDetails(
        TemplateMethodDetails currentDetails,
        ImmutableArray<AdditionalText> additionalText
    )
    {
        var text = additionalText.FirstOrDefault(x =>
            x.Path.EndsWith(
                currentDetails.AttributeDetails.TemplatePath ?? string.Empty,
                StringComparison.Ordinal
            )
        );

        if (text?.GetText()?.ToString() is not { } content)
        {
            return currentDetails.AttributeDetails;
        }

        var lexer = new Lexer(content);
        var tokens = lexer.ToArray();

        return currentDetails.AttributeDetails with
        {
            Template = content,
            Tokens = tokens,
        };
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

    private static TemplateMethodDetails BuildTemplateDetails(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken token
    )
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)ctx.TargetNode;

        // get the namespace
        var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(
            methodDeclarationSyntax,
            cancellationToken: token
        )!;
        var ns = methodSymbol.ContainingNamespace.ToDisplayString();

        // get all usings
        var usings = methodDeclarationSyntax
            .TryGetUsings()
            .Add(SF.UsingDirective(SF.ParseName(" System")))
            .Add(SF.UsingDirective(SF.ParseName(" System.Diagnostics.CodeAnalysis")))
            .Add(SF.UsingDirective(SF.ParseName(" Fluidic")));

        // get the method details
        var containingType = methodSymbol.ContainingType;
        var @class = containingType.Name;
        var name = methodDeclarationSyntax.Identifier.Text;
        var methodDetails = new MethodDetails(
            Name: name,
            MethodDeclaration: methodDeclarationSyntax,
            MethodSymbol: methodSymbol
        );

        // get the attribute details
        var attributeParts = new TemplateAttributeParts(methodDetails, ctx.SemanticModel);

        return new TemplateMethodDetails(
            Namespace: ns,
            Usings: usings,
            ClassName: @class,
            MethodDetails: methodDetails,
            AttributeDetails: attributeParts
        );
    }

    private static void GenerateStringTemplate(
        SourceProductionContext context,
        TemplateMethodDetails details
    )
    {
        using var writer = new StringWriter();
        var indentedWriter = new IndentedTextWriter(writer, "    ");

        WriteTemplateMethod(indentedWriter, details);

        context.AddSource(
            $"{details.ClassName}.{details.MethodDetails.Name}.g.cs",
            SourceText.From(writer.ToString(), Encoding.UTF8)
        );
    }
}
