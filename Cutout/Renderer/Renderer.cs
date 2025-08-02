using System.CodeDom.Compiler;

namespace Cutout;

internal static class Renderer
{
    internal static void WriteSyntax(
        this IndentedTextWriter writer,
        string template,
        Syntax syntax,
        bool includeWhitespaceReceiver
    )
    {
        switch (syntax)
        {
            case Syntax.VarStatement varStatement:
                writer.WriteLine($"var {varStatement.Assignment};");
                break;
            case Syntax.CallStatement callStatement:
                writer.WriteCallStatement(template, callStatement, includeWhitespaceReceiver);
                break;
            case Syntax.IfStatement ifStatement:
                writer.WriteConditionalStatement(
                    template,
                    ifStatement,
                    "if (",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.ElseIfStatement elseIfStatement:
                writer.WriteConditionalStatement(
                    template,
                    elseIfStatement,
                    "else if (",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.ElseStatement elseStatement:
                writer.WriteLine("else");
                writer.WriteExpressions(
                    template,
                    elseStatement.Expressions,
                    includeWhitespaceReceiver
                );
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
        writer.Write(rawText.Value.ToSpan(template).ToString());
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
        writer.Write(renderableExpression.Value.ToSpan(template).ToString());
        writer.WriteLine(");");

        if (includeWhitespaceReceiver)
        {
            writer.Write("if ((");
            writer.Write(renderableExpression.Value.ToSpan(template).ToString());
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
                WriteSyntax(writer, template, syntax, includeWhitespaceReceiver);
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
        writer.Write(forStatement.Condition);
        writer.WriteLine(")");

        WriteExpressions(writer, template, forStatement.Expressions, includeWhitespaceReceiver);
    }

    private static void WriteCallStatement(
        this IndentedTextWriter writer,
        string template,
        Syntax.CallStatement callStatement,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write(callStatement.Name);
        writer.Write("(builder,");
        writer.Write(callStatement.Parameters);
        if (!string.IsNullOrEmpty(callStatement.LeadingWhitespace))
        {
            writer.Write(includeWhitespaceReceiver ? ",whitespace + \"" : ",\"");
            writer.Write(callStatement.LeadingWhitespace);
            writer.Write("\"");
        }
        writer.WriteLine(");");
    }
}
