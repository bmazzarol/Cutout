namespace Cutout;

internal static class Lexer
{
    internal static TokenList Tokenize(in ReadOnlySpan<char> text)
    {
        var tokens = new TokenList();
        var i = 0;
        var line = 1;
        var column = 1;
        var length = text.Length;

        while (
            i < length
            && (
                TryProcessNewline(text, ref i, ref line, ref column, length, tokens)
                || TryProcessCodeDelimiters(text, ref i, line, ref column, length, tokens)
                || TryProcessWhitespace(text, ref i, line, ref column, length, tokens)
                || TryProcessRawText(text, ref i, line, ref column, length, tokens)
            )
        )
        { // collect tokens
        }

        tokens.Add(Token.Eof);
        return tokens;
    }

    private static bool TryProcessNewline(
        ReadOnlySpan<char> text,
        ref int i,
        ref int line,
        ref int column,
        int length,
        TokenList tokens
    )
    {
        var c = text[i];
        var currentPosition = new CharPosition(line, column, i);

        switch (c)
        {
            case '\r' when i + 1 < length && text[i + 1] == '\n':
                tokens.Add(
                    new Token(
                        currentPosition,
                        new CharPosition(line, column + 1, i + 1),
                        TokenType.Newline
                    )
                );
                i += 2;
                line++;
                column = 1;
                return true;
            case '\n':
                tokens.Add(new Token(currentPosition, currentPosition, TokenType.Newline));
                i++;
                line++;
                column = 1;
                return true;
            default:
                return false;
        }
    }

    private static bool TryProcessCodeDelimiters(
        ReadOnlySpan<char> text,
        ref int i,
        int line,
        ref int column,
        int length,
        TokenList tokens
    )
    {
        var c = text[i];
        var currentPosition = new CharPosition(line, column, i);

        switch (c)
        {
            case '{' when i + 1 < length && text[i + 1] == '{':
                tokens.Add(
                    new Token(
                        currentPosition,
                        new CharPosition(line, column + 1, i + 1),
                        TokenType.CodeEnter
                    )
                );
                i += 2;
                column += 2;
                return true;
            case '}' when i + 1 < length && text[i + 1] == '}':
                tokens.Add(
                    new Token(
                        currentPosition,
                        new CharPosition(line, column + 1, i + 1),
                        TokenType.CodeExit
                    )
                );
                i += 2;
                column += 2;
                return true;
            default:
                return false;
        }
    }

    private static bool TryProcessWhitespace(
        ReadOnlySpan<char> text,
        ref int i,
        int line,
        ref int column,
        int length,
        TokenList tokens
    )
    {
        var c = text[i];

        if (
            !char.IsWhiteSpace(c)
            || c == '\n'
            || (c == '\r' && i + 1 < length && text[i + 1] == '\n')
        )
        {
            return false;
        }

        var start = i;
        var columnStart = column;

        while (
            i < length
            && char.IsWhiteSpace(text[i])
            && text[i] != '\n'
            && !(text[i] == '\r' && i + 1 < length && text[i + 1] == '\n')
        )
        {
            i++;
            column++;
        }

        tokens.Add(
            new Token(
                new CharPosition(line, columnStart, start),
                new CharPosition(line, column - 1, i - 1),
                TokenType.Whitespace
            )
        );

        return true;
    }

    private static bool TryProcessRawText(
        ReadOnlySpan<char> text,
        ref int i,
        int line,
        ref int column,
        int length,
        TokenList tokens
    )
    {
        var start = i;
        var columnStart = column;

        while (
            i < length
            && !(
                char.IsWhiteSpace(text[i])
                || (text[i] == '{' && i + 1 < length && text[i + 1] == '{')
                || (text[i] == '}' && i + 1 < length && text[i + 1] == '}')
                || text[i] == '\n'
                || (text[i] == '\r' && i + 1 < length && text[i + 1] == '\n')
            )
        )
        {
            i++;
            column++;
        }

        tokens.Add(
            new Token(
                new CharPosition(line, columnStart, start),
                new CharPosition(line, column - 1, i - 1),
                TokenType.Raw
            )
        );

        return true;
    }
}
