using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Cutout.Extensions;
using Cutout.Parser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban.Parsing;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cutout;

[Generator]
public sealed partial class TemplateSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("TemplateAttribute.g", TemplateAttributeSourceCode);
        });

        var templateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Cutout.TemplateAttribute",
            IsTemplateMethod,
            BuildTemplateDetails
        );

        context.RegisterSourceOutput(templateProvider, GenerateTemplate);
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
            .Add(SF.UsingDirective(SF.ParseName(" Cutout")));

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

    private static void GenerateTemplate(
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
