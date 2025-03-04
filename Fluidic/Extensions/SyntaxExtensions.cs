using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fluidic.Extensions;

internal static class SyntaxExtensions
{
    public static SyntaxList<UsingDirectiveSyntax> TryGetUsings(this SyntaxNode node)
    {
        SyntaxList<UsingDirectiveSyntax> result = SyntaxFactory.List<UsingDirectiveSyntax>();
        foreach (var ancestor in node.Ancestors(ascendOutOfTrivia: false))
            result = ancestor switch
            {
                NamespaceDeclarationSyntax syntax => result.AddRange(syntax.Usings),
                CompilationUnitSyntax syntax => result.AddRange(syntax.Usings),
                _ => result,
            };
        return result;
    }

    public static bool IsNamedAttribute(this AttributeSyntax syntax, string name)
    {
        if (string.Equals(syntax.Name.ToString(), name, StringComparison.Ordinal))
        {
            return true;
        }

        var parts = name.Split('.');

        return parts.Length == 2
            && string.Equals(syntax.Name.ToString(), parts[1], StringComparison.Ordinal);
    }
}
