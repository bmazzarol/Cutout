using Fluidic.Exceptions;
using Fluidic.Extensions;
using Scriban.Parsing;

namespace Fluidic.Parser;

internal static partial class TemplateParser
{
    private sealed class IfParseContext : IParseContext
    {
        private IfParseContext() { }

        public static IfParseContext Instance { get; } = new();

        public bool ShouldBreak(
            ReadOnlySpan<Token> tokens,
            ReadOnlySpan<char> template,
            ref int index
        )
        {
            var current = tokens[index];

            if (!current.ToSpan(template).SequenceEqual(Identifiers.Else))
            {
                return current.ToSpan(template).SequenceEqual(Identifiers.End);
            }

            index -= 2; // rewind so that we can parse the else statement
            return true;
        }

        public string MessageOnNoBreak => "else or end not found";
        public bool ShouldSkipLeadingNewline => true;
        public bool ShouldSkipTrailingNewline => true;
    }

    private static Syntax.IfStatement ParseIfStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        ExtractCodeTokens(tokens, template, ref index, out var start, out var end);
        var condition = start.ToSpan(template, end: end).ToString();

        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ParseException(
                start,
                start.ToSpan(template).ToString(),
                "if statement condition not found"
            );
        }

        var expressions = ParseInternal(
            tokens,
            template,
            context: IfParseContext.Instance,
            ref index
        );
        return new Syntax.IfStatement(condition, expressions.ToArray());
    }

    private static Syntax ParseElseStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        var current = tokens[index];

        IReadOnlyList<Syntax> expressions;
        if (current.Type == TokenType.CodeExit)
        {
            index++;
            expressions = ParseInternal(
                tokens,
                template,
                context: EndParseContext.Instance,
                ref index
            );
            return new Syntax.ElseStatement(expressions);
        }

        if (!current.ToSpan(template).SequenceEqual(Identifiers.If))
        {
            throw new ParseException(
                current,
                current.ToSpan(template).ToString(),
                "else statement must be followed by if statement"
            );
        }

        index++;

        ExtractCodeTokens(tokens, template, ref index, out var start, out var end);
        var condition = start.ToSpan(template, end: end).ToString();

        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ParseException(
                start,
                start.ToSpan(template).ToString(),
                "else if statement condition not found"
            );
        }

        expressions = ParseInternal(tokens, template, context: IfParseContext.Instance, ref index);
        return new Syntax.ElseIfStatement(condition, expressions.ToArray());
    }
}
