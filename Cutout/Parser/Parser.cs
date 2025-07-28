namespace Cutout;

internal static partial class Parser
{
    private static readonly char[] Var = ['v', 'a', 'r'];
    private static readonly char[] Call = ['c', 'a', 'l', 'l'];
    private static readonly char[] If = ['i', 'f'];
    private static readonly char[] Else = ['e', 'l', 's', 'e'];
    private static readonly char[] ElseIf = ['e', 'l', 's', 'e', 'i', 'f'];
    private static readonly char[] End = ['e', 'n', 'd'];
    private static readonly char[] For = ['f', 'o', 'r'];
    private static readonly char[] Foreach = ['f', 'o', 'r', 'e', 'a', 'c', 'h'];
    private static readonly char[] While = ['w', 'h', 'i', 'l', 'e'];
    private static readonly char[] Break = ['b', 'r', 'e', 'a', 'k'];
    private static readonly char[] Continue = ['c', 'o', 'n', 't', 'i', 'n', 'u', 'e'];
    private static readonly char[] Return = ['r', 'e', 't', 'u', 'r', 'n'];

    [ThreadStatic]
    private static Context? _threadContext;

    internal static SyntaxList Parse(TokenList tokens, string template)
    {
        var context = _threadContext;
        if (context == null)
        {
            context = new Context(tokens, template);
            _threadContext = context;
        }
        else
        {
            context.Reset(tokens, template);
        }
        return ParseInternal(context, BreakOn.Eof, out _);
    }

    private enum BreakOn
    {
        Eof,
        End,
    }

    private static SyntaxList ParseInternal(
        Context context,
        BreakOn breakOn,
        out CodeBlockContext? endBlockContext
    )
    {
        CodeBlockContext? blockContext = null;
        var syntaxList = new SyntaxList();

        while (context.MoveNext())
        {
            switch (context.Current.Type)
            {
                case TokenType.Eof:
                    if (breakOn != BreakOn.Eof)
                    {
                        throw context.Failure("Unexpected end of file, expected a {{ end }} block");
                    }

                    continue;
                case TokenType.CodeEnter:
                {
                    blockContext ??= new CodeBlockContext(context);
                    ParseCodeBlock(context, blockContext);

                    if (TryParseRecursiveEnd(context, blockContext, breakOn))
                    {
                        endBlockContext = blockContext;
                        return syntaxList;
                    }

                    if (blockContext.IsOnlyIdentifier(Break))
                    {
                        syntaxList.Add(Syntax.BreakStatement.Instance);
                    }
                    else if (blockContext.IsOnlyIdentifier(Continue))
                    {
                        syntaxList.Add(Syntax.ContinueStatement.Instance);
                    }
                    else if (blockContext.IsOnlyIdentifier(Return))
                    {
                        syntaxList.Add(Syntax.ReturnStatement.Instance);
                    }
                    else if (TryParseVarStatement(context, blockContext, out var varSyntax))
                    {
                        syntaxList.Add(varSyntax);
                    }
                    else if (TryParseCallStatement(context, blockContext, out var callSyntax))
                    {
                        syntaxList.Add(callSyntax);
                    }
                    else if (blockContext.IsIdentifier(While))
                    {
                        ParseConditionalStatement(
                            context,
                            blockContext,
                            out var condition,
                            out var expressions,
                            out _
                        );
                        var syntax = new Syntax.WhileStatement(condition, expressions);
                        syntaxList.Add(syntax);
                    }
                    else if (blockContext.IsIdentifier(For))
                    {
                        ParseConditionalStatement(
                            context,
                            blockContext,
                            out var condition,
                            out var expressions,
                            out _
                        );
                        var syntax = new Syntax.ForStatement(condition, expressions);
                        syntaxList.Add(syntax);
                    }
                    else if (blockContext.IsIdentifier(Foreach))
                    {
                        ParseConditionalStatement(
                            context,
                            blockContext,
                            out var condition,
                            out var expressions,
                            out _
                        );
                        var syntax = new Syntax.ForeachStatement(condition, expressions);
                        syntaxList.Add(syntax);
                    }
                    else if (TryParseIfStatement(context, blockContext, out var ifSyntax))
                    {
                        syntaxList.Add(ifSyntax);
                    }
                    else
                    {
                        var codeTokens = context.Tokens.GetRange(
                            blockContext.StartIndex,
                            blockContext.Length
                        );
                        var syntax = new Syntax.RenderableExpression(codeTokens);
                        syntaxList.Add(syntax);
                    }
                    break;
                }
                default:
                {
                    var rawText = ParseRawText(context);
                    syntaxList.Add(rawText);
                    break;
                }
            }
        }

        endBlockContext = null;
        return syntaxList;
    }

    private static bool TryParseRecursiveEnd(
        Context context,
        CodeBlockContext codeBlockContext,
        BreakOn breakOn
    )
    {
        if (
            !codeBlockContext.IsIdentifier(End)
            && !codeBlockContext.IsIdentifier(ElseIf)
            && !codeBlockContext.IsIdentifier(Else)
        )
        {
            return false;
        }

        if (!codeBlockContext.IsOnlyIdentifier(ElseIf))
        {
            throw context.Failure(
                codeBlockContext.IdentifierIndex,
                $"{{{{ {codeBlockContext.Identifier.ToString()} }}}} statement should only contain the identifier"
            );
        }

        if (breakOn == BreakOn.Eof)
        {
            throw context.Failure(
                codeBlockContext.IdentifierIndex,
                $"{{{{ {codeBlockContext.Identifier.ToString()} }}}} found but not expected"
            );
        }

        return true;
    }

    private static bool TryParseVarStatement(
        Context context,
        CodeBlockContext blockContext,
        out Syntax.VarStatement? syntax
    )
    {
        if (!blockContext.IsIdentifier(Var))
        {
            syntax = null;
            return false;
        }

        if (blockContext.IsJustIdentifier)
        {
            throw context.Failure(
                blockContext.IdentifierIndex,
                "{{ var }} declaration requires an assignment expression"
            );
        }

        syntax = new Syntax.VarStatement(blockContext.RemainingTokens());
        return true;
    }

    private static bool TryParseCallStatement(
        Context context,
        CodeBlockContext blockContext,
        out Syntax.CallStatement? syntax
    )
    {
        if (!blockContext.IsIdentifier(Call))
        {
            syntax = null;
            return false;
        }

        if (blockContext.IsJustIdentifier)
        {
            throw context.Failure(
                blockContext.IdentifierIndex,
                "{{ call }} statement requires parameters"
            );
        }

        var text = blockContext.RemainingTokens().ToSpan(context.Template).Trim().ToString();
        var callParts = text.Split('(', ')');

        if (callParts.Length != 3 || string.IsNullOrWhiteSpace(callParts[0]))
        {
            throw context.Failure(
                blockContext.IdentifierIndex,
                "{{ call }} statement requires a function name and () with optional parameters"
            );
        }

        syntax = new Syntax.CallStatement(
            callParts[0],
            callParts[1]
                .Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray()
        );
        return true;
    }

    private static void ParseConditionalStatement(
        Context context,
        CodeBlockContext blockContext,
        out TokenList condition,
        out SyntaxList expressions,
        out CodeBlockContext? endBlockContext
    )
    {
        if (blockContext.IsJustIdentifier)
        {
            throw context.Failure(
                blockContext.IdentifierIndex,
                $"{{{{ {blockContext.Identifier.ToString()} }}}} statement requires a condition"
            );
        }

        condition = blockContext.RemainingTokens();
        expressions = ParseInternal(context, BreakOn.End, out endBlockContext);
    }

    private static bool TryParseIfStatement(
        Context context,
        CodeBlockContext blockContext,
        out Syntax.IfStatement? syntax
    )
    {
        if (!blockContext.IsIdentifier(If))
        {
            syntax = null;
            return false;
        }

        if (blockContext.IsJustIdentifier)
        {
            throw context.Failure(
                blockContext.IdentifierIndex,
                "{{ if }} statement requires a condition"
            );
        }

        ParseConditionalStatement(
            context,
            blockContext,
            out var condition,
            out var expressions,
            out var endBlockContext
        );

        List<Syntax.ElseIfStatement>? elseifStatements = null;
        Syntax.ElseStatement? elseStatement = null;
        do
        {
            if (endBlockContext!.IsIdentifier(ElseIf))
            {
                if (elseStatement != null)
                {
                    throw context.Failure(
                        endBlockContext.IdentifierIndex,
                        "Cannot have {{ elseif }} after {{ else }} in an {{ if }} statement"
                    );
                }

                ParseConditionalStatement(
                    context,
                    endBlockContext,
                    out var elseifCondition,
                    out var elseifExpressions,
                    out endBlockContext
                );
                elseifStatements ??= [];
                elseifStatements.Add(
                    new Syntax.ElseIfStatement(elseifCondition, elseifExpressions)
                );
            }
            else if (endBlockContext.IsIdentifier(Else))
            {
                if (elseStatement != null)
                {
                    throw context.Failure(
                        endBlockContext.IdentifierIndex,
                        "Only one {{ else }} is allowed within an {{ if }} statement"
                    );
                }

                var elseExpressions = ParseInternal(context, BreakOn.End, out endBlockContext);
                elseStatement = new Syntax.ElseStatement(elseExpressions);
            }
        } while (!endBlockContext!.IsIdentifier(End));

        syntax = new Syntax.IfStatement(condition, expressions, elseifStatements, elseStatement);

        return true;
    }

    private static Syntax.RawText ParseRawText(Context context)
    {
        var start = context.Index;
        while (context.MoveNext())
        {
            if (!context.Current.IsRawToken())
            {
                break;
            }
        }
        var rawTokens = context.Tokens.GetRange(start, context.Index - start);
        context.Index--;
        return new Syntax.RawText(rawTokens);
    }
}
