namespace Cutout.Parser;

internal abstract record Syntax
{
    private Syntax() { }

    public virtual bool SuppressTrailingNewline => true;

    /// <summary>
    /// Represents a raw text node in the template
    /// </summary>
    public sealed record RawText(string Value) : Syntax
    {
        public override bool SuppressTrailingNewline => false;
    }

    /// <summary>
    /// Represents a renderable expression in the template
    /// </summary>
    public sealed record RenderableExpression(string Value) : Syntax
    {
        public override bool SuppressTrailingNewline => false;
    }

    public abstract record WrappingExpressionsStatement(IReadOnlyList<Syntax> Expressions) : Syntax;

    public abstract record ConditionalStatement(string Condition, IReadOnlyList<Syntax> Expressions)
        : WrappingExpressionsStatement(Expressions);

    /// <summary>
    /// Represents a conditional if statement in the template
    /// </summary>
    /// <param name="Condition">condition to evaluate</param>
    /// <param name="Expressions">expressions to render if the condition is true</param>
    public sealed record IfStatement(string Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    /// <summary>
    /// Represents a conditional if else statement in the template
    /// </summary>
    /// <param name="Condition">condition to evaluate</param>
    /// <param name="Expressions">expressions to render if the condition is true</param>
    public sealed record ElseIfStatement(string Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    /// <summary>
    /// Represents a conditional else statement in the template
    /// </summary>
    /// <param name="Expressions"></param>
    public sealed record ElseStatement(IReadOnlyList<Syntax> Expressions)
        : WrappingExpressionsStatement(Expressions);

    /// <summary>
    /// Represents a for statement in the template
    /// </summary>
    /// <param name="Condition">condition to evaluate; must be a valid for statement in C#</param>
    /// <param name="Expressions">expressions to render if the condition is true</param>
    public sealed record ForStatement(string Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    /// <summary>
    /// Represents a foreach statement in the template
    /// </summary>
    /// <param name="Condition">condition to evaluate; must be a valid foreach statement in C#</param>
    /// <param name="Expressions">expressions to render if the condition is true</param>
    public sealed record ForeachStatement(string Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    /// <summary>
    /// Represents a while statement in the template
    /// </summary>
    /// <param name="Condition">condition to evaluate; must be a valid while statement in C#</param>
    /// <param name="Expressions">expressions to render if the condition is true</param>
    public sealed record WhileStatement(string Condition, IReadOnlyList<Syntax> Expressions)
        : ConditionalStatement(Condition, Expressions);

    /// <summary>
    /// Represents a var statement in the template
    /// </summary>
    /// <param name="Assignment">var statement to assign</param>
    public sealed record VarStatement(string Assignment) : Syntax
    {
        public override bool SuppressTrailingNewline => true;
    }

    /// <summary>
    /// Represents a call to another template method
    /// </summary>
    /// <param name="Name">name of the template method to call</param>
    /// <param name="Parameters">parameters to pass to the template method</param>
    public sealed record CallStatement(string Name, string Parameters) : Syntax
    {
        public override bool SuppressTrailingNewline => true;
    }

    /// <summary>
    /// Represents a break statement in the template
    /// </summary>
    public sealed record BreakStatement : Syntax
    {
        private BreakStatement() { }

        public static BreakStatement Instance { get; } = new();
    }

    /// <summary>
    /// Represents a continue statement in the template
    /// </summary>
    public sealed record ContinueStatement : Syntax
    {
        private ContinueStatement() { }

        public static ContinueStatement Instance { get; } = new();
    }

    /// <summary>
    /// Represents a return statement in the template
    /// </summary>
    public sealed record ReturnStatement : Syntax
    {
        private ReturnStatement() { }

        public static ReturnStatement Instance { get; } = new();
    }

    /// <summary>
    /// Represents a non-operation in the template, such as end
    /// </summary>
    public sealed record NoOp : Syntax
    {
        private NoOp() { }

        public static NoOp Instance { get; } = new();
    }
}
