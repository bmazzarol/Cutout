using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Cutout.Exceptions;

namespace Cutout;

internal static partial class Parser
{
    private sealed class Context : IEnumerator<Token>
    {
        public TokenList Tokens { get; private set; }
        public string Template { get; private set; }
        public int Index { get; set; }

        public Context(TokenList tokens, string template)
        {
            Tokens = tokens;
            Template = template;
            Index = -1;
        }

        public bool MoveNext()
        {
            if (Index + 1 >= Tokens.Count)
            {
                return false;
            }

            Index++;
            return true;
        }

        [ExcludeFromCodeCoverage]
        public void Reset()
        {
            Index = -1;
        }

        [ExcludeFromCodeCoverage]
        object IEnumerator.Current => Current;

        public Token Current => Tokens[Index];

        public ParseException Failure(string reason)
        {
            return Current.Failure(Template, reason);
        }

        public ParseException Failure(int index, string reason)
        {
            return Tokens[index].Failure(Template, reason);
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            Reset();
        }

        public void Reset(TokenList tokens, string template)
        {
            Tokens = tokens;
            Template = template;
            Index = -1;
        }
    }
}
