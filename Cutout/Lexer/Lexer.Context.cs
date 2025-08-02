using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Cutout;

internal static partial class Lexer
{
    public sealed class Context : IEnumerator<char>
    {
        public int Index { get; private set; }

        public int Column { get; private set; }

        public bool TryAdvance(int count)
        {
            if (Index + count >= Template.Length)
            {
                return false;
            }

            Index += count;
            Column += count;
            return true;
        }

        public int Line { get; private set; }

        public void AdvanceNewline(bool windowsNewline = false)
        {
            if (windowsNewline)
            {
                Index++;
            }

            Line++;
            Column = 0;
        }

        public string Template { get; private set; } = null!;

        public ReadOnlySpan<char> Peek(int count)
        {
            return Index + count >= Template.Length
                ? Template.AsSpan(Index)
                : Template.AsSpan(Index, count);
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            Reset();
        }

        public bool MoveNext()
        {
            return TryAdvance(count: 1);
        }

        public void Reset()
        {
            Index = -1;
            Column = 0;
            Line = 1;
        }

        public void Reset(string template)
        {
            Template = template;
            Reset();
        }

        public char Current => Template[Index];

        [ExcludeFromCodeCoverage]
        object IEnumerator.Current => Current;

        public CharPosition CurrentPosition => new(Line, Column, Index);

        public Token Token(TokenType type, int count)
        {
            var start = CurrentPosition;
            var end = new CharPosition(
                start.Line,
                start.Column + count,
                CurrentPosition.Offset + count
            );
            return new(start, end, type);
        }
    }
}
