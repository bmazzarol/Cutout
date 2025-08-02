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

        _syntaxes = new Lazy<SyntaxList>(() =>
        {
            var tokens = Lexer.Tokenize(Template ?? string.Empty);
            var tokensWithWsSuppressed = Lexer.ApplyWhitespaceSuppression(tokens);
            return Parser.Parse(tokensWithWsSuppressed, Template ?? string.Empty);
        });
    }
}
