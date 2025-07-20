namespace Cutout.Tests;

public class LexerTests
{
    [Fact(DisplayName = "Whitespace can be tokenized")]
    public void Case1()
    {
        const string text = "  \t\n\r  ";
        var tokens = Lexer.Tokenize(text.AsSpan());

        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(new CharPosition(1, 1, 0), token.Start);
                Assert.Equal(new CharPosition(1, 3, 2), token.End);
                Assert.Equal("  \t", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Newline, token.Type);
                Assert.Equal(new CharPosition(1, 4, 3), token.Start);
                Assert.Equal(new CharPosition(1, 4, 3), token.End);
                Assert.Equal("\n", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(new CharPosition(2, 1, 4), token.Start);
                Assert.Equal(new CharPosition(2, 3, 6), token.End);
                Assert.Equal("\r  ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Eof, token.Type);
                Assert.Equal(new CharPosition(0, 0, -1), token.Start);
                Assert.Equal(new CharPosition(0, 0, -1), token.End);
                Assert.Equal(string.Empty, token.ToSpan(text.AsSpan()).ToString());
            }
        );
    }

    [Fact(DisplayName = "Code blocks can be tokenized")]
    public void Case2()
    {
        const string text = "{{ code block }}";
        var tokens = Lexer.Tokenize(text.AsSpan());
        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(TokenType.CodeEnter, token.Type);
                Assert.Equal(0, token.Start.Offset);
                Assert.Equal(1, token.End.Offset);
                Assert.Equal("{{", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(2, token.Start.Offset);
                Assert.Equal(2, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(3, token.Start.Offset);
                Assert.Equal(6, token.End.Offset);
                Assert.Equal("code", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(7, token.Start.Offset);
                Assert.Equal(7, token.End.Offset);

                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(8, token.Start.Offset);
                Assert.Equal(12, token.End.Offset);
                Assert.Equal("block", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(13, token.Start.Offset);
                Assert.Equal(13, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.CodeExit, token.Type);
                Assert.Equal(14, token.Start.Offset);
                Assert.Equal(15, token.End.Offset);
                Assert.Equal("}}", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Eof, token.Type);
                Assert.Equal(-1, token.Start.Offset);
                Assert.Equal(-1, token.End.Offset);
            }
        );
    }

    [Fact(DisplayName = "Raw text can be tokenized")]
    public void Case3()
    {
        const string text = "This is some raw text";
        var tokens = Lexer.Tokenize(text.AsSpan());
        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(0, token.Start.Offset);
                Assert.Equal(3, token.End.Offset);
                Assert.Equal("This", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(4, token.Start.Offset);
                Assert.Equal(4, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(5, token.Start.Offset);
                Assert.Equal(6, token.End.Offset);
                Assert.Equal("is", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(7, token.Start.Offset);
                Assert.Equal(7, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(8, token.Start.Offset);
                Assert.Equal(11, token.End.Offset);
                Assert.Equal("some", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(12, token.Start.Offset);
                Assert.Equal(12, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(13, token.Start.Offset);
                Assert.Equal(15, token.End.Offset);
                Assert.Equal("raw", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(16, token.Start.Offset);
                Assert.Equal(16, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(17, token.Start.Offset);
                Assert.Equal(20, token.End.Offset);
                Assert.Equal("text", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Eof, token.Type);
                Assert.Equal(-1, token.Start.Offset);
                Assert.Equal(-1, token.End.Offset);
            }
        );
    }

    [Fact(DisplayName = "Mixed text can be tokenized")]
    public void Case4()
    {
        const string text = "This is {{ code block }}\n some raw text\r\n and {{ more code }}";
        var tokens = Lexer.Tokenize(text.AsSpan());
        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(0, token.Start.Offset);
                Assert.Equal(3, token.End.Offset);
                Assert.Equal("This", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(4, token.Start.Offset);
                Assert.Equal(4, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(5, token.Start.Offset);
                Assert.Equal(6, token.End.Offset);
                Assert.Equal("is", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(7, token.Start.Offset);
                Assert.Equal(7, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.CodeEnter, token.Type);
                Assert.Equal(8, token.Start.Offset);
                Assert.Equal(9, token.End.Offset);
                Assert.Equal("{{", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(10, token.Start.Offset);
                Assert.Equal(10, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(11, token.Start.Offset);
                Assert.Equal(14, token.End.Offset);
                Assert.Equal("code", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(15, token.Start.Offset);
                Assert.Equal(15, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(16, token.Start.Offset);
                Assert.Equal(20, token.End.Offset);
                Assert.Equal("block", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(21, token.Start.Offset);
                Assert.Equal(21, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.CodeExit, token.Type);
                Assert.Equal(22, token.Start.Offset);
                Assert.Equal(23, token.End.Offset);
                Assert.Equal("}}", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Newline, token.Type);
                Assert.Equal(24, token.Start.Offset);
                Assert.Equal(24, token.End.Offset);
                Assert.Equal("\n", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(25, token.Start.Offset);
                Assert.Equal(25, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(26, token.Start.Offset);
                Assert.Equal(29, token.End.Offset);
                Assert.Equal("some", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(30, token.Start.Offset);
                Assert.Equal(30, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(31, token.Start.Offset);
                Assert.Equal(33, token.End.Offset);
                Assert.Equal("raw", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(34, token.Start.Offset);
                Assert.Equal(34, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(35, token.Start.Offset);
                Assert.Equal(38, token.End.Offset);
                Assert.Equal("text", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Newline, token.Type);
                Assert.Equal(39, token.Start.Offset);
                Assert.Equal(40, token.End.Offset);
                Assert.Equal("\r\n", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(41, token.Start.Offset);
                Assert.Equal(41, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(42, token.Start.Offset);
                Assert.Equal(44, token.End.Offset);
                Assert.Equal("and", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(45, token.Start.Offset);
                Assert.Equal(45, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.CodeEnter, token.Type);
                Assert.Equal(46, token.Start.Offset);
                Assert.Equal(47, token.End.Offset);
                Assert.Equal("{{", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(48, token.Start.Offset);
                Assert.Equal(48, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(49, token.Start.Offset);
                Assert.Equal(52, token.End.Offset);
                Assert.Equal("more", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(53, token.Start.Offset);
                Assert.Equal(53, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(54, token.Start.Offset);
                Assert.Equal(57, token.End.Offset);
                Assert.Equal("code", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(58, token.Start.Offset);
                Assert.Equal(58, token.End.Offset);
                Assert.Equal(" ", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.CodeExit, token.Type);
                Assert.Equal(59, token.Start.Offset);
                Assert.Equal(60, token.End.Offset);
                Assert.Equal("}}", token.ToSpan(text.AsSpan()).ToString());
            },
            token =>
            {
                Assert.Equal(TokenType.Eof, token.Type);
                Assert.Equal(-1, token.Start.Offset);
                Assert.Equal(-1, token.End.Offset);
            }
        );
    }
}
