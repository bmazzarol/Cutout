using Scriban.Parsing;

namespace Fluidic.Extensions;

internal static class TokenExtensions
{
    internal static ReadOnlySpan<char> ToSpan(
        this Token token,
        ReadOnlySpan<char> template,
        Token? end = null
    )
    {
        if (token.Type == TokenType.Eof)
        {
            return [];
        }

        var endToken = end ?? token;

        return template.Slice(
            token.Start.Offset,
            length: Math.Max(endToken.End.Offset - token.Start.Offset + 1, 0)
        );
    }
}
