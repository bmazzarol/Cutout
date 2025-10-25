using Microsoft.CodeAnalysis;

namespace Cutout;

internal sealed record TemplateAttributeParts
{
    public string? Template { get; }

    private readonly Lazy<SyntaxList> _syntaxes;

    internal SyntaxList Syntaxes => _syntaxes.Value;

    public TemplateAttributeParts(MethodDetails details, SemanticModel ctxSemanticModel)
    {
        var attributes = details
            .MethodDeclaration.AttributeLists.SelectMany(list => list.Attributes)
            .ToArray();

        var arguments = attributes
            .Single(x => x.IsNamedAttribute("Cutout.Template"))
            .ArgumentList!.Arguments;

        var template = ctxSemanticModel.GetConstantValue(arguments[0].Expression);

        Template = template.HasValue ? template.Value?.ToString() : string.Empty;

        _syntaxes = BuildSyntax();
    }

    public TemplateAttributeParts(string template)
    {
        Template = template;
        _syntaxes = BuildSyntax();
    }

    private Lazy<SyntaxList> BuildSyntax()
    {
        return new Lazy<SyntaxList>(() =>
        {
            var tokens = Lexer.Tokenize(Template ?? string.Empty);
            var tokensWithWsSuppressed = Lexer.ApplyWhitespaceSuppression(tokens);
            return Parser.Parse(tokensWithWsSuppressed, Template ?? string.Empty);
        });
    }

    public bool Equals(TemplateAttributeParts? other)
    {
        if (other is null)
            return false;
        return ReferenceEquals(this, other)
            || string.Equals(Template, other.Template, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Template != null ? StringComparer.Ordinal.GetHashCode(Template) : 0;
    }
}
