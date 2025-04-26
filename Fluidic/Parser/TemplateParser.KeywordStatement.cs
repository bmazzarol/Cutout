using Fluidic.Exceptions;
using Fluidic.Extensions;
using Scriban.Parsing;

namespace Fluidic.Parser;

internal static partial class TemplateParser
{
    private static void EnsureKeywordStatementOnly(
        string keyword,
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        in int index
    )
    {
        if (tokens[index].Type != TokenType.CodeExit)
        {
            throw new ParseException(
                tokens[index],
                tokens[index].ToSpan(template).ToString(),
                $"Expected only keyword '{keyword}'"
            );
        }
    }
}
