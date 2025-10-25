using System.CodeDom.Compiler;

namespace Cutout;

internal static class Renderer
{
    internal static void WriteSyntax(
        this IndentedTextWriter writer,
        string template,
        Syntax syntax,
        Syntax? lastSyntax,
        bool includeWhitespaceReceiver
    )
    {
        switch (syntax)
        {
            case Syntax.VarStatement varStatement:
                writer.WriteLine($"var {varStatement.Assignment.ToString(template)};");
                break;
            case Syntax.CallStatement callStatement:
                writer.WriteCallStatement(
                    lastSyntax is Syntax.RawText rt && rt.TryGetLeadingWhitespace(out var wst)
                        ? wst?.ToSpan(template).ToString()
                        : null,
                    callStatement,
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.IfStatement ifStatement:
                writer.WriteConditionalStatement(
                    template,
                    ifStatement,
                    "if (",
                    includeWhitespaceReceiver
                );

                foreach (var ifStatementElseIf in ifStatement.ElseIfs ?? [])
                {
                    writer.WriteConditionalStatement(
                        template,
                        ifStatementElseIf,
                        "else if (",
                        includeWhitespaceReceiver
                    );
                }

                if (ifStatement.Else != null)
                {
                    writer.WriteLine("else");
                    writer.WriteExpressions(
                        template,
                        ifStatement.Else.Expressions,
                        includeWhitespaceReceiver
                    );
                }

                break;
            case Syntax.ForStatement forStatement:
                writer.WriteConditionalStatement(
                    template,
                    forStatement,
                    "for (var ",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.ForeachStatement foreachStatement:
                writer.WriteConditionalStatement(
                    template,
                    foreachStatement,
                    "foreach (var ",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.WhileStatement whileStatement:
                writer.WriteConditionalStatement(
                    template,
                    whileStatement,
                    "while (",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.ContinueStatement:
                writer.WriteLine("continue;");
                break;
            case Syntax.BreakStatement:
                writer.WriteLine("break;");
                break;
            case Syntax.ReturnStatement:
                writer.WriteLine("return;");
                break;
            case Syntax.RawText rawText:
                writer.WriteRawText(template, rawText, includeWhitespaceReceiver);
                break;
            case Syntax.RenderableExpression renderableExpression:
                writer.WriteRenderableExpression(
                    template,
                    renderableExpression,
                    includeWhitespaceReceiver
                );
                break;
        }
    }

    private static void WriteRawText(
        this IndentedTextWriter writer,
        string template,
        Syntax.RawText rawText,
        bool includeWhitespaceReceiver
    )
    {
        var renderable = rawText.Value.ToString(template).Replace("\"", "\"\"");
        if (includeWhitespaceReceiver && rawText.ContainsNewLine)
        {
            writer.Write("builder.Append(Cutout.RenderUtilities.ApplyExtraWhitespace(@\"");
            writer.Write(renderable);
            writer.WriteLine("\", whitespace));");
        }
        else
        {
            writer.Write("builder.Append(@\"");
            writer.Write(renderable);
            writer.WriteLine("\");");
        }
    }

    private static void WriteRenderableExpression(
        this IndentedTextWriter writer,
        string template,
        Syntax.RenderableExpression renderableExpression,
        bool includeWhitespaceReceiver
    )
    {
        var renderable = renderableExpression.Value.ToString(template);
        if (includeWhitespaceReceiver)
        {
            writer.Write("builder.Append(Cutout.RenderUtilities.ApplyExtraWhitespace(");
            writer.Write(renderable);
            writer.WriteLine(", whitespace));");
        }
        else
        {
            writer.Write("builder.Append(");
            writer.Write(renderable);
            writer.WriteLine(");");
        }
    }

    private static void WriteExpressions(
        this IndentedTextWriter writer,
        string template,
        IReadOnlyList<Syntax> expressions,
        bool includeWhitespaceReceiver
    )
    {
        writer.WriteLine("{");
        using (writer.Indent())
        {
            for (var i = 0; i < expressions.Count; i++)
            {
                var syntax = expressions[i];
                WriteSyntax(
                    writer,
                    template,
                    syntax,
                    lastSyntax: i < expressions.Count - 1 ? expressions[i + 1] : null,
                    includeWhitespaceReceiver
                );
            }
        }
        writer.WriteLine("}");
    }

    private static void WriteConditionalStatement(
        this IndentedTextWriter writer,
        string template,
        Syntax.ConditionalStatement forStatement,
        string conditionalPrefix,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write(conditionalPrefix);
        writer.Write(forStatement.Condition.ToString(template));
        writer.WriteLine(")");

        WriteExpressions(writer, template, forStatement.Expressions, includeWhitespaceReceiver);
    }

    private static void WriteCallStatement(
        this IndentedTextWriter writer,
        string? whitespace,
        Syntax.CallStatement callStatement,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write(callStatement.Name);
        writer.Write("(builder");
        foreach (var parameter in callStatement.Parameters)
        {
            writer.Write(", ");
            writer.Write(parameter);
        }

        var hasWhitespace = whitespace?.Length > 0;
        if (includeWhitespaceReceiver || hasWhitespace)
        {
            writer.Write(", ");
            if (includeWhitespaceReceiver)
            {
                writer.Write("whitespace");
                if (hasWhitespace)
                {
                    writer.Write(" + ");
                }
            }

            if (hasWhitespace)
            {
                writer.Write("\"");
                writer.Write(whitespace);
                writer.Write("\"");
            }
        }
        writer.WriteLine(");");
    }
}
