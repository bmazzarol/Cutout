using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cutout;

internal sealed record TemplateMethodDetails(
    string? Namespace,
    SyntaxList<UsingDirectiveSyntax> Usings,
    ClassDetails ClassDetails,
    MethodDetails MethodDetails,
    TemplateAttributeParts AttributeDetails
)
{
    /// <summary>
    /// Identity of the generated source: every value the renderer writes,
    /// reduced to plain strings. Syntax nodes and symbols carry object
    /// identity rather than content, so the record's default equality is
    /// never satisfied after a recompute and the RegisterSourceOutput dedup
    /// step re-runs the generator for every template on every keystroke.
    /// </summary>
    private string? _identity;

    private string Identity =>
        _identity ??= BuildIdentity(Namespace, Usings, ClassDetails, MethodDetails, AttributeDetails);

    private static string BuildIdentity(
        string? ns,
        SyntaxList<UsingDirectiveSyntax> usings,
        ClassDetails classDetails,
        MethodDetails methodDetails,
        TemplateAttributeParts attributeDetails
    )
    {
        var builder = new StringBuilder();
        builder.Append(ns).Append('\u0001');
        foreach (var usingDirective in usings)
        {
            builder.Append(usingDirective.ToString()).Append('\u0001');
        }
        builder.Append(classDetails.Name).Append('\u0001');
        builder.Append(classDetails.ClassSymbol.DeclaredAccessibility).Append('\u0001');
        builder.Append(methodDetails.Name).Append('\u0001');
        foreach (var parameter in methodDetails.MethodSymbol.Parameters)
        {
            builder.Append(parameter.Type.ToDisplayString()).Append(' ').Append(parameter.Name).Append('\u0001');
        }
        builder.Append(attributeDetails.Template);
        return builder.ToString();
    }

    [ExcludeFromCodeCoverage]
    public bool Equals(TemplateMethodDetails? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other)
            || string.Equals(Identity, other.Identity, StringComparison.Ordinal);
    }

    [ExcludeFromCodeCoverage]
    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Identity);
    }
}

internal sealed record ClassDetails(string? Name, INamedTypeSymbol ClassSymbol);

internal sealed record MethodDetails(
    string? Name,
    MethodDeclarationSyntax MethodDeclaration,
    IMethodSymbol MethodSymbol
);
