using System.CodeDom.Compiler;
using Cutout.Extensions;
using Cutout.Parser;

namespace Cutout;

internal static class Renderer
{
    internal static void WriteSyntax(this IndentedTextWriter writer, Syntax syntax)
    {
        switch (syntax)
        {
            case Syntax.VarStatement varStatement:
                writer.WriteLine($"var {varStatement.Assignment};");
                break;
            case Syntax.CallStatement callStatement:
                writer.WriteCallStatement(callStatement);
                break;
            case Syntax.IfStatement ifStatement:
                writer.WriteConditionalStatement(ifStatement, "if (");
                break;
            case Syntax.ElseIfStatement elseIfStatement:
                writer.WriteConditionalStatement(elseIfStatement, "else if (");
                break;
            case Syntax.ElseStatement elseStatement:
                writer.WriteLine("else");
                writer.WriteExpressions(elseStatement.Expressions);
                break;
            case Syntax.ForStatement forStatement:
                writer.WriteConditionalStatement(forStatement, "for (var ");
                break;
            case Syntax.ForeachStatement foreachStatement:
                writer.WriteConditionalStatement(foreachStatement, "foreach (var ");
                break;
            case Syntax.WhileStatement whileStatement:
                writer.WriteConditionalStatement(whileStatement, "while (");
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
                writer.WriteRawText(rawText);
                break;
            case Syntax.RenderableExpression renderableExpression:
                writer.WriteRenderableExpression(renderableExpression);
                break;
        }
    }

    private static void WriteRawText(this IndentedTextWriter writer, Syntax.RawText rawText)
    {
        writer.Write("builder.Append(@\"");
        writer.Write(rawText.Value);
        writer.WriteLine("\");");
    }

    private static void WriteRenderableExpression(
        this IndentedTextWriter writer,
        Syntax.RenderableExpression renderableExpression
    )
    {
        writer.Write("builder.Append(");
        writer.Write(renderableExpression.Value);
        writer.WriteLine(");");
    }

    private static void WriteExpressions(
        this IndentedTextWriter writer,
        IReadOnlyList<Syntax> expressions
    )
    {
        writer.WriteLine("{");
        using (writer.Indent())
        {
            for (var i = 0; i < expressions.Count; i++)
            {
                var syntax = expressions[i];
                WriteSyntax(writer, syntax);
            }
        }
        writer.WriteLine("}");
    }

    private static void WriteConditionalStatement(
        this IndentedTextWriter writer,
        Syntax.ConditionalStatement forStatement,
        string conditionalPrefix
    )
    {
        writer.Write(conditionalPrefix);
        writer.Write(forStatement.Condition);
        writer.WriteLine(")");

        WriteExpressions(writer, forStatement.Expressions);
    }

    private static void WriteCallStatement(
        this IndentedTextWriter writer,
        Syntax.CallStatement callStatement
    )
    {
        writer.Write(callStatement.Name);
        writer.Write("(builder,");
        writer.Write(callStatement.Parameters);
        writer.WriteLine(");");
    }
}
