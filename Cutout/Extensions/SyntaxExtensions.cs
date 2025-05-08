using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cutout.Extensions;

internal static class SyntaxExtensions
{
    public static SyntaxList<UsingDirectiveSyntax> TryGetUsings(this SyntaxNode node)
    {
        var result = SyntaxFactory.List<UsingDirectiveSyntax>();
        return node.Ancestors(ascendOutOfTrivia: false)
            .Aggregate(
                result,
                static (current, ancestor) =>
                    ancestor switch
                    {
                        NamespaceDeclarationSyntax syntax => current.AddRange(syntax.Usings),
                        CompilationUnitSyntax syntax => current.AddRange(syntax.Usings),
                        _ => current,
                    }
            );
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
