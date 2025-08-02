using System.Runtime.InteropServices;
using Cutout.Exceptions;

namespace Cutout;

internal enum TokenType : byte
{
    /// <summary>
    /// End of file token
    /// </summary>
    Eof,

    /// <summary>
    /// Whitespace token
    /// </summary>
    Whitespace,

    /// <summary>
    /// Newline token
    /// </summary>
    Newline,

    /// <summary>
    /// Any text not a whitespace or newline
    /// </summary>
    Raw,

    /// <summary>
    /// Start of a render block "{{"
    /// </summary>
    RenderEnter,

    /// <summary>
    /// Start of a render block "{{-" such that any leading whitespace and newline are suppressed
    /// </summary>
    RenderSuppressWsEnter,

    /// <summary>
    /// End of a render block "}}"
    /// </summary>
    RenderExit,

    /// <summary>
    /// End of a render block "-}}" such that any trailing whitespace and newline are suppressed
    /// </summary>
    RenderSuppressWsExit,

    /// <summary>
    /// Start of a code block "{%"
    /// </summary>
    CodeEnter,

    /// <summary>
    /// Start of a code block "{%-" such that any leading whitespace and newline are suppressed
    /// </summary>
    CodeSuppressWsEnter,

    /// <summary>
    /// End of a code block "%}"
    /// </summary>
    CodeExit,

    /// <summary>
    /// End of a code block "-%}" such that any trailing whitespace and newline are suppressed
    /// </summary>
    CodeSuppressWsExit,
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct CharPosition(int Line, int Column, int Offset)
{
    public static readonly CharPosition Empty = new(0, 0, -1);

    public Token ToToken(TokenType type, int count)
    {
        return new Token(
            this,
            new CharPosition(Line, Column + count - 1, Offset + count - 1),
            type
        );
    }
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct Token(CharPosition Start, CharPosition End, TokenType Type)
{
    public static readonly Token Eof = new(CharPosition.Empty, CharPosition.Empty, TokenType.Eof);

    public bool IsRawToken()
    {
        return Type is TokenType.Raw or TokenType.Whitespace or TokenType.Newline;
    }

    public bool IsCodeBlockEnterToken()
    {
        return Type is TokenType.CodeEnter or TokenType.CodeSuppressWsEnter;
    }

    public bool IsCodeBlockExitToken()
    {
        return Type is TokenType.CodeExit or TokenType.CodeSuppressWsExit;
    }

    public bool IsRenderBlockEnterToken()
    {
        return Type is TokenType.RenderEnter or TokenType.RenderSuppressWsEnter;
    }

    public bool IsRenderBlockExitToken()
    {
        return Type is TokenType.RenderExit or TokenType.RenderSuppressWsExit;
    }

    public bool IsBlockEnterToken()
    {
        return Type
            is TokenType.RenderEnter
                or TokenType.RenderSuppressWsEnter
                or TokenType.CodeEnter
                or TokenType.CodeSuppressWsEnter;
    }

    public bool IsBlockExitToken()
    {
        return Type
            is TokenType.RenderExit
                or TokenType.RenderSuppressWsExit
                or TokenType.CodeExit
                or TokenType.CodeSuppressWsExit;
    }

    public override string ToString()
    {
        return $"{Type}({Start}:{End})";
    }

    public ReadOnlySpan<char> ToSpan(string template, Token? end = null)
    {
        if (Type == TokenType.Eof)
        {
            return [];
        }

        var endToken = end ?? this;
        return template
            .AsSpan()
            .Slice(Start.Offset, length: Math.Max(endToken.End.Offset - Start.Offset + 1, 0));
    }

    internal ParseException Failure(string template, string reason)
    {
        return new ParseException(this, ToSpan(template).ToString(), reason);
    }
}
