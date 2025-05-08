using Cutout.Exceptions;
using Cutout.Extensions;
using Scriban.Parsing;

namespace Cutout.Parser;

/// <summary>
/// Parser for the template syntax
/// </summary>
internal static partial class TemplateParser
{
    /// <summary>
    /// Given a list of tokens, parse them into a list of syntax nodes
    /// </summary>
    /// <param name="tokens">tokens to parse</param>
    /// <param name="template">parsed template</param>
    /// <returns>list of syntax nodes</returns>
    internal static IReadOnlyList<Syntax> Parse(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template
    )
    {
        var index = 0;
        return ParseInternal(tokens, template, context: null, ref index);
    }

    private static IReadOnlyList<Syntax> ParseInternal(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        IParseContext? context,
        ref int index
    )
    {
        var result = new List<Syntax>();
        for (; index < tokens.Length; index++)
        {
            var token = tokens[index];

            switch (token.Type)
            {
                case TokenType.Raw:
                {
                    var syntax = ParseRawText(
                        template,
                        token,
                        shouldSkipLeadingNewline: context?.ShouldSkipLeadingNewline is true
                            && result.Count == 0
                    );
                    if (syntax != Syntax.NoOp.Instance)
                    {
                        result.Add(syntax);
                    }
                    break;
                }
                case TokenType.CodeEnter:
                {
                    index++;

                    if (context?.ShouldBreak(tokens, template, ref index) is true)
                    {
                        if (
                            !context.ShouldSkipTrailingNewline
                            || result.Count == 0
                            || result[result.Count - 1] is not Syntax.RawText rawText
                        )
                        {
                            return result;
                        }

                        var rawTextSpan = rawText.Value.AsSpan();
                        rawTextSpan = TryRemoveTrailingNewline(rawTextSpan);

                        if (rawTextSpan.Length == 0)
                        {
                            result.RemoveAt(result.Count - 1);
                        }
                        else
                        {
                            result[result.Count - 1] = new Syntax.RawText(rawTextSpan.ToString());
                        }

                        return result;
                    }

                    var codeSyntax = ParseCode(tokens, template, ref index);
                    if (codeSyntax != Syntax.NoOp.Instance)
                    {
                        result.Add(codeSyntax);
                    }

                    break;
                }
                case TokenType.CodeExit:
                {
                    if (result.Count > 0 && result[result.Count - 1].SuppressTrailingNewline)
                    {
                        TrySkipWhitespace(tokens, template, ref index);
                    }
                    break;
                }
            }
        }

        if (context == null)
        {
            return result;
        }

        index -= 2;
        throw new ParseException(
            tokens[index],
            tokens[index].ToSpan(template).ToString(),
            context.MessageOnNoBreak
        );
    }

    private static Syntax ParseRawText(
        ReadOnlySpan<char> template,
        Token token,
        bool shouldSkipLeadingNewline
    )
    {
        var rawText = token.ToSpan(template);

        if (shouldSkipLeadingNewline)
        {
            rawText = TryRemoveLeadingNewline(rawText);
        }

        if (rawText.Length == 0)
        {
            return Syntax.NoOp.Instance;
        }

        var syntax = new Syntax.RawText(rawText.ToString());
        return syntax;
    }

    private static ReadOnlySpan<char> TryRemoveTrailingNewline(ReadOnlySpan<char> rawTextSpan)
    {
        if (rawTextSpan.Length < 2)
        {
            return rawTextSpan;
        }

        var last2 = rawTextSpan.Slice(rawTextSpan.Length - 2);
        if (last2.SequenceEqual(['\r', '\n']))
        {
            return rawTextSpan.Slice(0, rawTextSpan.Length - 2);
        }

        if (rawTextSpan[rawTextSpan.Length - 1] == '\n')
        {
            return rawTextSpan.Slice(0, rawTextSpan.Length - 1);
        }

        return rawTextSpan;
    }

    private static ReadOnlySpan<char> TryRemoveLeadingNewline(ReadOnlySpan<char> rawText)
    {
        var trimmed = rawText.TrimStart(' ');
        return trimmed.Length switch
        {
            > 0 when trimmed[0] == '\n' => trimmed.Slice(1),
            > 1 when trimmed[0] == '\r' && trimmed[1] == '\n' => trimmed.Slice(2),
            _ => rawText,
        };
    }

    private static Syntax ParseCode(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        var current = tokens[index];
        var span = current.ToSpan(template);

        if (span.SequenceEqual(Identifiers.Var))
        {
            index++;
            return ParseVarStatement(tokens, template, ref index);
        }

        if (span.SequenceEqual(Identifiers.Break))
        {
            EnsureKeywordStatementOnly("break", tokens, template, index: index + 1);
            TrySkipWhitespace(tokens, template, ref index);
            return Syntax.BreakStatement.Instance;
        }

        if (span.SequenceEqual(Identifiers.Continue))
        {
            EnsureKeywordStatementOnly("continue", tokens, template, index: index + 1);
            TrySkipWhitespace(tokens, template, ref index);
            return Syntax.ContinueStatement.Instance;
        }

        if (span.SequenceEqual(Identifiers.Return))
        {
            EnsureKeywordStatementOnly("return", tokens, template, index: index + 1);
            TrySkipWhitespace(tokens, template, ref index);
            return Syntax.ReturnStatement.Instance;
        }

        if (span.SequenceEqual(Identifiers.If))
        {
            index++;
            return ParseIfStatement(tokens, template, ref index);
        }

        if (span.SequenceEqual(Identifiers.Else))
        {
            index++;
            return ParseElseStatement(tokens, template, ref index);
        }

        if (span.SequenceEqual(Identifiers.Foreach))
        {
            index++;
            return ParseForeachStatement(tokens, template, ref index);
        }

        if (span.SequenceEqual(Identifiers.For))
        {
            index++;
            return ParseForStatement(tokens, template, ref index);
        }

        if (span.SequenceEqual(Identifiers.While))
        {
            index++;
            return ParseWhileStatement(tokens, template, ref index);
        }

        if (span.SequenceEqual(Identifiers.End))
        {
            index++;
            return Syntax.NoOp.Instance;
        }

        return ParseRenderableExpression(tokens, template, ref index);
    }

    private static Syntax.RenderableExpression ParseRenderableExpression(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        ExtractCodeTokens(tokens, template, ref index, out var start, out var end);
        var renderableExpression = start.ToSpan(template, end: end).ToString();
        return new Syntax.RenderableExpression(renderableExpression);
    }

    private static void ExtractCodeTokens(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index,
        out Token start,
        out Token end
    )
    {
        if (tokens[index].Type == TokenType.CodeEnter)
        {
            index++;
        }

        start = tokens[index];

        while (tokens.Length > index && tokens[index].Type != TokenType.CodeExit)
        {
            index++;
        }

        var current = tokens[index];
        if (current.Type != TokenType.CodeExit)
        {
            throw new ParseException(
                current,
                current.ToSpan(template).ToString(),
                "Code exit token not found"
            );
        }

        index--;

        end = tokens[index];
    }

    private static void TrySkipWhitespace(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        if (
            tokens.Length > index + 1
            && tokens[index + 1].Type == TokenType.Raw
            && tokens[index + 1].ToSpan(template).IsWhiteSpace()
        )
        {
            index++;
        }
    }

    private static void ExtractConditionalStatement(
        string keyword,
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        IParseContext context,
        ref int index,
        out string condition,
        out IReadOnlyList<Syntax> expressions
    )
    {
        ExtractCodeTokens(tokens, template, ref index, out var start, out var end);
        condition = start.ToSpan(template, end: end).ToString();

        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ParseException(
                start,
                start.ToSpan(template).ToString(),
                $"{keyword} statement condition not found"
            );
        }

        expressions = ParseInternal(tokens, template, context, ref index);
    }
}
