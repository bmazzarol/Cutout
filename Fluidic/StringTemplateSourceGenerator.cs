using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Fluidic.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
        });

        var refinedTypeDetailsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Fluidic.StringTemplateAttribute",
            IsTemplateMethod,
            BuildTemplateDetails
        );

        context.RegisterSourceOutput(refinedTypeDetailsProvider, GenerateStringTemplate);
    }

    private bool IsTemplateMethod(SyntaxNode syntax, CancellationToken token)
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

    private TemplateMethodDetails BuildTemplateDetails(
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
        var attributeParts = new StringTemplateAttributeParts(methodDetails, ctx.SemanticModel);

        return new TemplateMethodDetails(
            Namespace: ns,
            Usings: usings,
            ClassName: @class,
            MethodDetails: methodDetails,
            AttributeDetails: attributeParts
        );
    }

    private void GenerateStringTemplate(
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
