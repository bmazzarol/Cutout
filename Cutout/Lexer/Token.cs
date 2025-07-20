using System.Runtime.InteropServices;

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
    /// Start of a code block "{{"
    /// </summary>
    CodeEnter,

    /// <summary>
    /// End of a code block "}}"
    /// </summary>
    CodeExit,
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct CharPosition(int Line, int Column, int Offset)
{
    public static readonly CharPosition Empty = new(0, 0, -1);
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct Token(CharPosition Start, CharPosition End, TokenType Type)
{
    public static readonly Token Eof = new(CharPosition.Empty, CharPosition.Empty, TokenType.Eof);

    public bool IsRawToken()
    {
        return Type is TokenType.Raw or TokenType.Whitespace or TokenType.Newline;
    }

    public override string ToString()
    {
        return $"{Type}({Start}:{End})";
    }

    public ReadOnlySpan<char> ToSpan(in ReadOnlySpan<char> template, Token? end = null)
    {
        if (Type == TokenType.Eof)
        {
            return [];
        }

        var endToken = end ?? this;
        return template.Slice(
            Start.Offset,
            length: Math.Max(endToken.End.Offset - Start.Offset + 1, 0)
        );
    }
}
