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
        writer.Write("builder.Append(@\"");
        writer.Write(rawText.Value.ToString(template).Replace("\"", "\"\""));
        writer.WriteLine("\");");

        if (includeWhitespaceReceiver && rawText.ContainsNewLine)
        {
            writer.WriteLine("builder.Append(whitespace);");
        }
    }

    private static void WriteRenderableExpression(
        this IndentedTextWriter writer,
        string template,
        Syntax.RenderableExpression renderableExpression,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write("builder.Append(");
        writer.Write(renderableExpression.Value.ToString(template));
        writer.WriteLine(");");

        if (includeWhitespaceReceiver)
        {
            writer.Write("if ((");
            writer.Write(renderableExpression.Value.ToString(template));
            writer.WriteLine(").ToString().IndexOf('\\n') != -1)");
            writer.WriteLine("{");
            using (writer.Indent())
            {
                writer.WriteLine("builder.Append(whitespace);");
            }
            writer.WriteLine("}");
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
        if (!string.IsNullOrEmpty(whitespace))
        {
            writer.Write(includeWhitespaceReceiver ? ", whitespace + \"" : ",\"");
            writer.Write(whitespace);
            writer.Write("\"");
        }
        writer.WriteLine(");");
    }
}
