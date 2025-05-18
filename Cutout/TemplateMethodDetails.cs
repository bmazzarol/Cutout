using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cutout;

internal sealed record TemplateMethodDetails(
    string? Namespace,
    SyntaxList<UsingDirectiveSyntax> Usings,
    ClassDetails ClassDetails,
    MethodDetails MethodDetails,
    TemplateAttributeParts AttributeDetails
);

internal sealed record ClassDetails(string? Name, INamedTypeSymbol ClassSymbol);

internal sealed record MethodDetails(
    string? Name,
    MethodDeclarationSyntax MethodDeclaration,
    IMethodSymbol MethodSymbol
);
