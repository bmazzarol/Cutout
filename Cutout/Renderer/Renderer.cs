using System.CodeDom.Compiler;
using Cutout.Extensions;
using Cutout.Parser;

namespace Cutout;

internal static class Renderer
{
    internal static void WriteSyntax(
        this IndentedTextWriter writer,
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
                writer.WriteCallStatement(callStatement, includeWhitespaceReceiver);
                break;
            case Syntax.IfStatement ifStatement:
                writer.WriteConditionalStatement(ifStatement, "if (", includeWhitespaceReceiver);
                break;
            case Syntax.ElseIfStatement elseIfStatement:
                writer.WriteConditionalStatement(
                    elseIfStatement,
                    "else if (",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.ElseStatement elseStatement:
                writer.WriteLine("else");
                writer.WriteExpressions(elseStatement.Expressions, includeWhitespaceReceiver);
                break;
            case Syntax.ForStatement forStatement:
                writer.WriteConditionalStatement(
                    forStatement,
                    "for (var ",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.ForeachStatement foreachStatement:
                writer.WriteConditionalStatement(
                    foreachStatement,
                    "foreach (var ",
                    includeWhitespaceReceiver
                );
                break;
            case Syntax.WhileStatement whileStatement:
                writer.WriteConditionalStatement(
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
                writer.WriteRawText(rawText, includeWhitespaceReceiver);
                break;
            case Syntax.RenderableExpression renderableExpression:
                writer.WriteRenderableExpression(renderableExpression, includeWhitespaceReceiver);
                break;
        }
    }

    private static void WriteRawText(
        this IndentedTextWriter writer,
        Syntax.RawText rawText,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write("builder.Append(@\"");
        // writer.Write(rawText.Value);
        writer.WriteLine("\");");

        if (includeWhitespaceReceiver && rawText.ContainsNewline)
        {
            writer.WriteLine("builder.Append(whitespace);");
        }
    }

    private static void WriteRenderableExpression(
        this IndentedTextWriter writer,
        Syntax.RenderableExpression renderableExpression,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write("builder.Append(");
        writer.Write(renderableExpression.Value);
        writer.WriteLine(");");

        if (includeWhitespaceReceiver)
        {
            writer.Write("if ((");
            writer.Write(renderableExpression.Value);
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
                WriteSyntax(writer, syntax, includeWhitespaceReceiver);
            }
        }
        writer.WriteLine("}");
    }

    private static void WriteConditionalStatement(
        this IndentedTextWriter writer,
        Syntax.ConditionalStatement forStatement,
        string conditionalPrefix,
        bool includeWhitespaceReceiver
    )
    {
        writer.Write(conditionalPrefix);
        writer.Write(forStatement.Condition);
        writer.WriteLine(")");

        WriteExpressions(writer, forStatement.Expressions, includeWhitespaceReceiver);
    }

    private static void WriteCallStatement(
        this IndentedTextWriter writer,
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
