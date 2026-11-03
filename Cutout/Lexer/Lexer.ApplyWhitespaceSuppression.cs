namespace Cutout;

internal static partial class Lexer
{
    internal static TokenList ApplyWhitespaceSuppression(TokenList tokens)
    {
        var result = new TokenList();
        var i = 0;
        while (i < tokens.Count)
        {
            var token = tokens[i];
            switch (token.Type)
            {
                case TokenType.RenderSuppressWsEnter:
                case TokenType.CodeSuppressWsEnter:
                {
                    RemoveLeadingWhitespaceAndNewline(result);
                    result.Add(token);
                    i++;
                    break;
                }
                case TokenType.RenderSuppressWsExit:
                case TokenType.CodeSuppressWsExit:
                {
                    result.Add(token);
                    i = SkipTrailingWhitespaceAndNewline(tokens, i + 1);
                    break;
                }
                default:
                    result.Add(token);
                    i++;
                    break;
            }
        }
        return result;
    }

    private static void RemoveLeadingWhitespaceAndNewline(TokenList result)
    {
        while (result.Count > 0 &&
            (result[result.Count - 1].Type == TokenType.Whitespace || result[result.Count - 1].Type == TokenType.Newline))
        {
            result.RemoveAt(result.Count - 1);
        }
    }

    private static int SkipTrailingWhitespaceAndNewline(TokenList tokens, int startIndex)
    {
        var index = startIndex;
        while (index < tokens.Count &&
            (tokens[index].Type == TokenType.Whitespace || tokens[index].Type == TokenType.Newline))
        {
            index++;
        }
        return index;
    }
}
