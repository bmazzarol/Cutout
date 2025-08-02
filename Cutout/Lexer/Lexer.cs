namespace Cutout;

internal static partial class Lexer
{
    [ThreadStatic]
    private static Context? _context;

    internal static TokenList Tokenize(string text)
    {
        var tokens = new TokenList();
        _context ??= new Context();
        _context.Reset(text);

        var collectCount = 0;
        var isWhitespace = false;
        CharPosition? start = null;
        while (_context.MoveNext())
        {
            if (TryProcessNewline(_context, out var newLineToken))
            {
                FlushAndAdd(newLineToken);
            }
            else if (TryProcessCodeDelimiters(_context, out var codeToken))
            {
                FlushAndAdd(codeToken);
            }
            else
            {
                if (isWhitespace ^ char.IsWhiteSpace(_context.Current))
                {
                    FlushCollected();
                    isWhitespace = !isWhitespace;
                }

                start ??= _context.CurrentPosition;
                collectCount++;
            }
        }

        FlushAndAdd(Token.Eof);
        return tokens;

        void FlushCollected()
        {
            if (collectCount <= 0)
            {
                start = null;
                return;
            }

            tokens.Add(
                start!.Value.ToToken(
                    isWhitespace ? TokenType.Whitespace : TokenType.Raw,
                    collectCount
                )
            );
            start = null;
            collectCount = 0;
        }

        void FlushAndAdd(Token token)
        {
            FlushCollected();
            tokens.Add(token);
        }
    }

    private static readonly char[] WindowsNewline = ['\r', '\n'];
    private static readonly char[] UnixNewline = ['\n'];

    private static bool TryProcessNewline(Context context, out Token token)
    {
        if (context.Peek(2).SequenceEqual(WindowsNewline))
        {
            token = context.Token(TokenType.Newline, 1);
            context.AdvanceNewline(windowsNewline: true);
            return true;
        }

        if (context.Peek(1).SequenceEqual(UnixNewline))
        {
            token = context.Token(TokenType.Newline, 0);
            context.AdvanceNewline(windowsNewline: false);
            return true;
        }

        token = default;
        return false;
    }

    private static readonly char[] RenderSuppressWsEnter = ['{', '{', '-'];
    private static readonly char[] RenderEnter = ['{', '{'];
    private static readonly char[] RenderSuppressWsExit = ['-', '}', '}'];
    private static readonly char[] RenderExit = ['}', '}'];
    private static readonly char[] CodeSuppressWsEnter = ['{', '%', '-'];
    private static readonly char[] CodeEnter = ['{', '%'];
    private static readonly char[] CodeSuppressWsExit = ['-', '%', '}'];
    private static readonly char[] CodeExit = ['%', '}'];

    private static bool TryProcessCodeDelimiters(Context context, out Token token)
    {
        if (
            context.Current != '{'
            && context.Current != '%'
            && context.Current != '-'
            && context.Current != '}'
        )
        {
            token = default;
            return false;
        }

        if (context.Peek(3).SequenceEqual(RenderSuppressWsEnter))
        {
            token = context.Token(TokenType.RenderSuppressWsEnter, 2);
            return context.TryAdvance(count: 2);
        }

        if (context.Peek(2).SequenceEqual(RenderEnter))
        {
            token = context.Token(TokenType.RenderEnter, 1);
            return context.TryAdvance(count: 1);
        }

        if (context.Peek(3).SequenceEqual(RenderSuppressWsExit))
        {
            token = context.Token(TokenType.RenderSuppressWsExit, 2);
            return context.TryAdvance(count: 2);
        }

        if (context.Peek(2).SequenceEqual(RenderExit))
        {
            token = context.Token(TokenType.RenderExit, 1);
            return context.TryAdvance(count: 1);
        }

        if (context.Peek(3).SequenceEqual(CodeSuppressWsEnter))
        {
            token = context.Token(TokenType.CodeSuppressWsEnter, 2);
            return context.TryAdvance(count: 2);
        }

        if (context.Peek(2).SequenceEqual(CodeEnter))
        {
            token = context.Token(TokenType.CodeEnter, 1);
            return context.TryAdvance(count: 1);
        }

        if (context.Peek(3).SequenceEqual(CodeSuppressWsExit))
        {
            token = context.Token(TokenType.CodeSuppressWsExit, 2);
            return context.TryAdvance(count: 2);
        }

        if (context.Peek(2).SequenceEqual(CodeExit))
        {
            token = context.Token(TokenType.CodeExit, 1);
            return context.TryAdvance(count: 1);
        }

        token = default;
        return false;
    }
}
