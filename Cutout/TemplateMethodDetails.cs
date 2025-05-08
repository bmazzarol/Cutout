using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cutout;

/// <summary>
/// Provides details about the template method
/// </summary>
/// <param name="Namespace">namespace of the method</param>
/// <param name="Usings">using directives of the method</param>
internal sealed record TemplateMethodDetails(
    string? Namespace,
    SyntaxList<UsingDirectiveSyntax> Usings,
    string? ClassName,
    MethodDetails MethodDetails,
    TemplateAttributeParts AttributeDetails
);

internal sealed record MethodDetails(
    string? Name,
    MethodDeclarationSyntax MethodDeclaration,
    IMethodSymbol MethodSymbol
);
