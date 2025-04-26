using Fluidic.Extensions;
using Fluidic.Parser;
using Microsoft.CodeAnalysis;
using Scriban.Parsing;

namespace Fluidic;

public sealed partial class TemplateSourceGenerator
{
    private sealed record TemplateAttributeParts
    {
        public string? Template { get; init; }

        internal IReadOnlyList<Syntax> Syntaxes { get; init; } = [];

        public TemplateAttributeParts(MethodDetails details, SemanticModel ctxSemanticModel)
        {
            var attributes = details
                .MethodDeclaration.AttributeLists.SelectMany(list => list.Attributes)
                .ToArray();

            var arguments = attributes
                .Single(x => x.IsNamedAttribute("Fluidic.Template"))
                .ArgumentList!.Arguments;

            var template = ctxSemanticModel.GetConstantValue(arguments[0].Expression);

            Template = template.HasValue ? template.Value?.ToString() : string.Empty;

            var lexer = new Lexer(Template!);
            var tokens = lexer.ToArray();
            Syntaxes = TemplateParser.Parse(tokens, Template.AsSpan());
        }
    }
}
