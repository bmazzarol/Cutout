using Scriban.Parsing;

namespace Cutout.Parser;

internal static partial class TemplateParser
{
    private static Syntax.ForStatement ParseForStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        ExtractConditionalStatement(
            "for",
            tokens,
            template,
            EndParseContext.InstanceWithSkipLeadingNewline,
            ref index,
            out var condition,
            out var expressions
        );
        return new Syntax.ForStatement(condition, expressions);
    }

    private static Syntax.ForeachStatement ParseForeachStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        ExtractConditionalStatement(
            "foreach",
            tokens,
            template,
            EndParseContext.InstanceWithSkipLeadingNewline,
            ref index,
            out var condition,
            out var expressions
        );
        return new Syntax.ForeachStatement(condition, expressions);
    }

    private static Syntax.WhileStatement ParseWhileStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        ExtractConditionalStatement(
            "while",
            tokens,
            template,
            EndParseContext.InstanceWithSkipLeadingNewline,
            ref index,
            out var condition,
            out var expressions
        );
        return new Syntax.WhileStatement(condition, expressions);
    }
}
