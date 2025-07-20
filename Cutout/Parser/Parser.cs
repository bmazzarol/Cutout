using Cutout.Exceptions;
using Cutout.Extensions;

namespace Cutout;

internal static class Parser
{
    private static readonly char[] Var = ['v', 'a', 'r'];
    private static readonly char[] Call = ['c', 'a', 'l', 'l'];
    private static readonly char[] If = ['i', 'f'];
    private static readonly char[] Else = ['e', 'l', 's', 'e'];
    private static readonly char[] End = ['e', 'n', 'd'];
    private static readonly char[] For = ['f', 'o', 'r'];
    private static readonly char[] Foreach = ['f', 'o', 'r', 'e', 'a', 'c', 'h'];
    private static readonly char[] While = ['w', 'h', 'i', 'l', 'e'];
    private static readonly char[] Break = ['b', 'r', 'e', 'a', 'k'];
    private static readonly char[] Continue = ['c', 'o', 'n', 't', 'i', 'n', 'u', 'e'];
    private static readonly char[] Return = ['r', 'e', 't', 'u', 'r', 'n'];

    internal static SyntaxList Parse(TokenList tokens, in ReadOnlySpan<char> template)
    {
        var index = 0;
        return ParseInternal(tokens, in template, ref index);
    }

    private static SyntaxList ParseInternal(
        TokenList tokens,
        in ReadOnlySpan<char> template,
        ref int index,
        params char[][] breakOnAny
    )
    {
        var syntaxList = new SyntaxList();
        var tokenCount = tokens.Count;

        while (index < tokenCount)
        {
            var token = tokens[index];
            switch (token.Type)
            {
                case TokenType.Eof:
                    if (breakOnAny.Length > 0)
                    {
                        throw new ParseException(
                            token,
                            token.ToSpan(in template).ToString(),
                            breakOnAny.Length == 1
                                ? $"Unexpected end of file, expected a {{{{ {new string(breakOnAny[0])} }}}} block"
                                : $"Unexpected end of file, expected one of {{{{ {string.Join(", ", breakOnAny.Select(b => new string(b)))} }}}} blocks"
                        );
                    }

                    index++;
                    continue;
                case TokenType.CodeEnter:
                {
                    ParseCodeBlock(
                        tokens,
                        in template,
                        ref index,
                        out var start,
                        out var count,
                        out var identifierIndex,
                        out var isJustIdentifier
                    );

                    var identifier = tokens[identifierIndex].ToSpan(in template);

                    if (isJustIdentifier && breakOnAny.Length > 0)
                    {
                        for (var i = 0; i < breakOnAny.Length; i++)
                        {
                            var breakOn = breakOnAny[i];
                            if (identifier.SequenceEqual(breakOn))
                            {
                                return syntaxList;
                            }
                        }
                    }

                    if (isJustIdentifier && identifier.SequenceEqual(Break))
                    {
                        syntaxList.Add(Syntax.BreakStatement.Instance);
                    }
                    else if (isJustIdentifier && identifier.SequenceEqual(Continue))
                    {
                        syntaxList.Add(Syntax.ContinueStatement.Instance);
                    }
                    else if (isJustIdentifier && identifier.SequenceEqual(Return))
                    {
                        syntaxList.Add(Syntax.ReturnStatement.Instance);
                    }
                    else if (identifier.SequenceEqual(Var))
                    {
                        var syntax = ParseVarStatement(ref identifier);
                        syntaxList.Add(syntax);
                    }
                    else if (identifier.SequenceEqual(Call))
                    {
                        var syntax = ParseCallStatement(ref identifier, in template);
                        syntaxList.Add(syntax);
                    }
                    else if (identifier.SequenceEqual(While))
                    {
                        ParseConditionalStatement(
                            ref identifier,
                            in template,
                            ref index,
                            out var condition,
                            out var expressions
                        );
                        var syntax = new Syntax.WhileStatement(condition, expressions);
                        syntaxList.Add(syntax);
                    }
                    else if (identifier.SequenceEqual(For))
                    {
                        ParseConditionalStatement(
                            ref identifier,
                            in template,
                            ref index,
                            out var condition,
                            out var expressions
                        );
                        var syntax = new Syntax.ForStatement(condition, expressions);
                        syntaxList.Add(syntax);
                    }
                    else if (identifier.SequenceEqual(Foreach))
                    {
                        ParseConditionalStatement(
                            ref identifier,
                            in template,
                            ref index,
                            out var condition,
                            out var expressions
                        );
                        var syntax = new Syntax.ForeachStatement(condition, expressions);
                        syntaxList.Add(syntax);
                    }
                    else
                    {
                        var codeTokens = tokens.GetRange(start, count);
                        syntaxList.Add(new Syntax.RenderableExpression(codeTokens));
                    }
                    break;

                    TokenList RemainingTokens() =>
                        tokens.GetRange(identifierIndex + 1, count - identifierIndex);

                    Syntax.VarStatement ParseVarStatement(ref ReadOnlySpan<char> identifier)
                    {
                        if (isJustIdentifier)
                        {
                            throw new ParseException(
                                tokens[identifierIndex],
                                identifier.ToString(),
                                "Variable declaration requires an expression"
                            );
                        }

                        var assignmentTokens = RemainingTokens();
                        var syntax = new Syntax.VarStatement(assignmentTokens);
                        return syntax;
                    }

                    Syntax.CallStatement ParseCallStatement(
                        ref ReadOnlySpan<char> identifier,
                        in ReadOnlySpan<char> template
                    )
                    {
                        if (isJustIdentifier)
                        {
                            throw new ParseException(
                                tokens[identifierIndex],
                                identifier.ToString(),
                                "Call statement requires parameters"
                            );
                        }

                        var callTokens = RemainingTokens();
                        var text = callTokens.ToSpan(in template).Trim().ToString();
                        var callParts = text.Split(
                            ['(', ')'],
                            StringSplitOptions.RemoveEmptyEntries
                        );

                        if (callParts.Length != 2 || string.IsNullOrWhiteSpace(callParts[0]))
                        {
                            throw new ParseException(
                                tokens[identifierIndex],
                                identifier.ToString(),
                                "Call statement requires a function name and () with optional parameters"
                            );
                        }

                        return new Syntax.CallStatement(
                            callParts[0],
                            callParts[1].Split(',').Select(p => p.Trim()).ToArray()
                        );
                    }

                    void ParseConditionalStatement(
                        ref ReadOnlySpan<char> identifier,
                        in ReadOnlySpan<char> template,
                        ref int index,
                        out TokenList condition,
                        out SyntaxList expressions
                    )
                    {
                        if (isJustIdentifier)
                        {
                            throw new ParseException(
                                tokens[identifierIndex],
                                identifier.ToString(),
                                $"{identifier.ToString()} statement requires a condition"
                            );
                        }

                        condition = RemainingTokens();
                        expressions = ParseInternal(
                            tokens,
                            in template,
                            ref index,
                            breakOnAny: End
                        );
                    }
                }
                default:
                {
                    var rawText = ParseRawText(tokens, ref index);
                    syntaxList.Add(rawText);
                    break;
                }
            }
        }
        return syntaxList;
    }

    private static void ParseCodeBlock(
        TokenList tokens,
        in ReadOnlySpan<char> template,
        ref int index,
        out int start,
        out int count,
        out int identifierIndex,
        out bool isJustIdentifier
    )
    {
        start = ++index;
        var tokenCount = tokens.Count;
        var rawTextCount = 0;
        identifierIndex = -1;
        var codeExitIndex = -1;

        while (index < tokenCount)
        {
            var token = tokens[index];
            rawTextCount += token.Type == TokenType.Raw ? 1 : 0;

            if (token.Type == TokenType.CodeExit)
            {
                codeExitIndex = index;
                break;
            }

            if (token.Type == TokenType.CodeEnter)
            {
                throw new ParseException(
                    token,
                    token.ToSpan(in template).ToString(),
                    "Nested code blocks are not allowed"
                );
            }

            if (identifierIndex < 0 && token.Type == TokenType.Raw)
            {
                identifierIndex = index;
            }

            index++;
        }

        if (identifierIndex < 0)
        {
            throw new ParseException(
                tokens[start],
                tokens[start].ToSpan(in template).ToString(),
                "Code block is empty"
            );
        }

        if (codeExitIndex < 0)
        {
            throw new ParseException(
                tokens[tokenCount - 1],
                tokens[tokenCount - 1].ToSpan(in template).ToString(),
                "Code exit token not found"
            );
        }

        count = index - start;
        index++; // Move past the CodeExit token
        isJustIdentifier = rawTextCount == 1;
    }

    private static Syntax.RawText ParseRawText(TokenList tokens, ref int index)
    {
        var start = index;
        while (index < tokens.Count)
        {
            var type = tokens[index];
            if (!type.IsRawToken())
                break;
            index++;
        }
        var rawTokens = tokens.GetRange(start, index - start);
        return new Syntax.RawText(rawTokens);
    }
}
