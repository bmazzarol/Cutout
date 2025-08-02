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
        var index = result.Count - 1;
        if (index >= 0 && result[index].Type == TokenType.Whitespace)
        {
            result.RemoveAt(index--);
        }
        if (index >= 0 && result[index].Type == TokenType.Newline)
        {
            result.RemoveAt(index);
        }
    }

    private static int SkipTrailingWhitespaceAndNewline(TokenList tokens, int startIndex)
    {
        var index = startIndex;
        if (index < tokens.Count && tokens[index].Type == TokenType.Whitespace)
        {
            index++;
        }
        if (index < tokens.Count && tokens[index].Type == TokenType.Newline)
        {
            index++;
        }
        return index;
    }
}
