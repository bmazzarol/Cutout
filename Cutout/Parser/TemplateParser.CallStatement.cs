using Cutout.Exceptions;
using Cutout.Extensions;
using Scriban.Parsing;

namespace Cutout.Parser;

internal static partial class TemplateParser
{
    private static Syntax.CallStatement ParseCallStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        TryExtractWhitespace(tokens, template,  index, out var whitespace);
        ExtractCodeTokens(tokens, template, ref index, out var start, out var end);

        var fullExpression = start.ToSpan(template, end: end).ToString();
        var parts = fullExpression.Split(['(', ')'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ParseException(
                tokens[index],
                tokens[index].ToSpan(template).ToString(),
                "Invalid call statement. Expected format: 'MethodName(...)'."
            );
        }

        var methodName = parts[0].Trim();
        if (string.IsNullOrEmpty(methodName))
        {
            throw new ParseException(
                tokens[index],
                tokens[index].ToSpan(template).ToString(),
                "Invalid call statement. Expected format: 'MethodName(...)'."
            );
        }

        var parameters = parts[1].Trim();

        TrySkipWhitespace(tokens, template, ref index);
        return new Syntax.CallStatement(methodName, parameters, whitespace);
    }

    private static void TryExtractWhitespace(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        in int index,
        out string whitespace
    )
    {
        whitespace = string.Empty;

        var lastToken = tokens[Math.Max(index - 3, 0)];
        if (lastToken.Type != TokenType.Raw)
        {
            return;
        }

        var lastTokenSpan = lastToken.ToSpan(template);
        var lastNewLineIndex = lastTokenSpan.LastIndexOf('\n');
        if (lastNewLineIndex == -1)
        {
            return;
        }

        var newLineIndex = lastNewLineIndex + 1;
        var whitespaceSlice = lastTokenSpan.Slice(newLineIndex);

        if (whitespaceSlice.IsEmpty || !whitespaceSlice.IsWhiteSpace())
        {
            return;
        }

        whitespace = whitespaceSlice.ToString();
    }
}
