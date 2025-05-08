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
        return new Syntax.CallStatement(methodName, parameters);
    }
}
