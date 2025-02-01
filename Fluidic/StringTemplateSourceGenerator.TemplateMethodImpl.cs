using System;
using System.CodeDom.Compiler;
using System.Linq;
using Fluidic.Extensions;
using Scriban.Parsing;

namespace Fluidic;

public sealed partial class StringTemplateSourceGenerator
{
    private static void WriteTemplateMethod(IndentedTextWriter writer, TemplateMethodDetails model)
    {
        WriteNamespaceParts(writer, model);

        writer.WriteLine();

        writer.Write("public static partial class ");
        writer.Write(model.ClassName);
        writer.WriteLine();

        writer.WriteLine("{");
        using (writer.Indent())
        {
            writer.Write("public static partial void ");
            writer.Write(model.MethodDetails.Name);
            writer.Write("(this ");
            for (var index = 0; index < model.MethodDetails.MethodSymbol.Parameters.Length; index++)
            {
                var parameter = model.MethodDetails.MethodSymbol.Parameters[index];
                writer.Write(parameter.Type);
                writer.Write(" ");
                writer.Write(parameter.Name);

                if (index < model.MethodDetails.MethodSymbol.Parameters.Length - 1)
                {
                    writer.Write(", ");
                }
            }

            writer.WriteLine(")");

            writer.WriteLine("{");
            using (writer.Indent())
            {
                foreach (var token in model.AttributeDetails.Tokens)
                {
                    WriteToken(writer, model.AttributeDetails.Template, token);
                }
            }
            writer.WriteLine("}");
        }
        writer.WriteLine("}");
    }

    private static void WriteToken(IndentedTextWriter writer, string? template, Token token)
    {
        switch (token.Type)
        {
            case TokenType.Invalid:
                break;
            case TokenType.FrontMatterMarker:
                break;
            case TokenType.CodeEnter:
                break;
            case TokenType.LiquidTagEnter:
                break;
            case TokenType.CodeExit:
                break;
            case TokenType.LiquidTagExit:
                break;
            case TokenType.Raw:
                writer.Write("builder.Append(\"");
                writer.Write(
                    template!.Substring(
                        token.Start.Offset,
                        length: token.End.Offset - token.Start.Offset + 1
                    )
                );
                writer.WriteLine("\");");
                break;
            case TokenType.Escape:
                break;
            case TokenType.EscapeEnter:
                break;
            case TokenType.EscapeExit:
                break;
            case TokenType.NewLine:
                break;
            case TokenType.Whitespace:
                break;
            case TokenType.WhitespaceFull:
                break;
            case TokenType.Comment:
                break;
            case TokenType.CommentMulti:
                break;
            case TokenType.IdentifierSpecial:
                break;
            case TokenType.Identifier:
                writer.Write("builder.Append(");
                writer.Write(
                    template!.Substring(
                        token.Start.Offset,
                        length: token.End.Offset - token.Start.Offset + 1
                    )
                );
                writer.WriteLine(");");
                break;
            case TokenType.Integer:
                break;
            case TokenType.HexaInteger:
                break;
            case TokenType.BinaryInteger:
                break;
            case TokenType.Float:
                break;
            case TokenType.String:
                break;
            case TokenType.ImplicitString:
                break;
            case TokenType.VerbatimString:
                break;
            case TokenType.SemiColon:
                break;
            case TokenType.Arroba:
                break;
            case TokenType.Caret:
                break;
            case TokenType.DoubleCaret:
                break;
            case TokenType.Colon:
                break;
            case TokenType.Equal:
                break;
            case TokenType.VerticalBar:
                break;
            case TokenType.PipeGreater:
                break;
            case TokenType.Exclamation:
                break;
            case TokenType.DoubleAmp:
                break;
            case TokenType.DoubleVerticalBar:
                break;
            case TokenType.Amp:
                break;
            case TokenType.Question:
                break;
            case TokenType.DoubleQuestion:
                break;
            case TokenType.QuestionDot:
                break;
            case TokenType.DoubleEqual:
                break;
            case TokenType.ExclamationEqual:
                break;
            case TokenType.Less:
                break;
            case TokenType.Greater:
                break;
            case TokenType.LessEqual:
                break;
            case TokenType.GreaterEqual:
                break;
            case TokenType.Divide:
                break;
            case TokenType.DoubleDivide:
                break;
            case TokenType.Asterisk:
                break;
            case TokenType.Plus:
                break;
            case TokenType.Minus:
                break;
            case TokenType.Percent:
                break;
            case TokenType.DoubleLessThan:
                break;
            case TokenType.DoubleGreaterThan:
                break;
            case TokenType.Comma:
                break;
            case TokenType.Dot:
                break;
            case TokenType.DoubleDot:
                break;
            case TokenType.TripleDot:
                break;
            case TokenType.DoubleDotLess:
                break;
            case TokenType.OpenParen:
                break;
            case TokenType.CloseParen:
                break;
            case TokenType.OpenBrace:
                break;
            case TokenType.CloseBrace:
                break;
            case TokenType.OpenBracket:
                break;
            case TokenType.CloseBracket:
                break;
            case TokenType.Custom:
                break;
            case TokenType.Custom1:
                break;
            case TokenType.Custom2:
                break;
            case TokenType.Custom3:
                break;
            case TokenType.Custom4:
                break;
            case TokenType.Custom5:
                break;
            case TokenType.Custom6:
                break;
            case TokenType.Custom7:
                break;
            case TokenType.Custom8:
                break;
            case TokenType.Custom9:
                break;
            case TokenType.Eof:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void WriteNamespaceParts(IndentedTextWriter writer, TemplateMethodDetails model)
    {
        writer.WriteLine("// <auto-generated/>");
        writer.WriteLine("#nullable enable");
        writer.WriteLine();

        foreach (
            var usingDirective in model.Usings.OrderBy(
                x => x.Name.ToString(),
                StringComparer.Ordinal
            )
        )
        {
            writer.WriteLine(usingDirective);
        }

        writer.WriteLine();
        writer.Write("namespace ");
        writer.Write(model.Namespace);
        writer.WriteLine(";");
    }
}
