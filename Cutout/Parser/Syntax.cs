namespace Cutout;

internal abstract record Syntax
{
    private Syntax() { }

    internal sealed record RawText(TokenList Value) : Syntax
    {
        public bool ContainsNewLine => Value.Exists(static x => x.Type == TokenType.Newline);

        /// <summary>
        /// Returns the leading whitespace if it exists and the previous token is a newline
        /// </summary>
        /// <param name="leadingWhitespace">leading whitespace token if it exists</param>
        /// <returns>>true if leading whitespace exists, otherwise false</returns>
        public bool TryGetLeadingWhitespace(out Token? leadingWhitespace)
        {
            leadingWhitespace = null;
            var count = Value.Count;
            if (count == 0 || Value[count - 1].Type != TokenType.Whitespace)
            {
                return false;
            }

            if (count < 2 || Value[count - 2].Type != TokenType.Newline)
            {
                return false;
            }

            leadingWhitespace = Value[count - 1];
            return true;
        }
    }

    internal sealed record RenderableExpression(TokenList Value) : Syntax;

    internal abstract record WrappingExpressionsStatement(IReadOnlyList<Syntax> Expressions)
        : Syntax;

    internal abstract record ConditionalStatement(
        TokenList Condition,
        IReadOnlyList<Syntax> Expressions
    ) : WrappingExpressionsStatement(Expressions);

    internal sealed record IfStatement(
        TokenList Condition,
        IReadOnlyList<Syntax> Expressions,
        IReadOnlyList<ElseIfStatement>? ElseIfs = null,
        ElseStatement? Else = null
    ) : ConditionalStatement(Condition, Expressions);

    internal sealed record ElseIfStatement(TokenList Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    internal sealed record ElseStatement(IReadOnlyList<Syntax> Expressions)
        : WrappingExpressionsStatement(Expressions);

    internal sealed record ForStatement(TokenList Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    internal sealed record ForeachStatement(TokenList Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    internal sealed record WhileStatement(TokenList Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    internal sealed record VarStatement(TokenList Assignment) : Syntax;

    internal sealed record CallStatement(string Name, IReadOnlyList<string> Parameters) : Syntax;

    internal sealed record BreakStatement : Syntax
    {
        private BreakStatement() { }

        internal static BreakStatement Instance { get; } = new();
    }

    internal sealed record ContinueStatement : Syntax
    {
        private ContinueStatement() { }

        internal static ContinueStatement Instance { get; } = new();
    }

    internal sealed record ReturnStatement : Syntax
    {
        private ReturnStatement() { }

        internal static ReturnStatement Instance { get; } = new();
    }

    internal sealed record NoOp : Syntax
    {
        private NoOp() { }

        internal static NoOp Instance { get; } = new();
    }
}
