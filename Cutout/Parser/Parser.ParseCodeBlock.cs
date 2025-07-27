namespace Cutout;

internal static partial class Parser
{
    private sealed class CodeBlockContext
    {
        private Context Context { get; set; }
        public int StartIndex { get; private set; }
        public int CodeExitIndex { get; set; }
        public int Length { get; set; }
        public int RawTextCount { get; set; }
        public int IdentifierIndex { get; set; }

        public ReadOnlySpan<char> Identifier =>
            Context.Tokens[IdentifierIndex].ToSpan(Context.Template);
        public bool IsJustIdentifier => RawTextCount == 1;

        public CodeBlockContext(Context context)
        {
            Context = context;
            Reset(context);
        }

        public bool IsOnlyIdentifier(in ReadOnlySpan<char> match)
        {
            return IsJustIdentifier && Identifier.SequenceEqual(match);
        }

        public bool IsIdentifier(in ReadOnlySpan<char> match)
        {
            return Identifier.SequenceEqual(match);
        }

        public TokenList RemainingTokens()
        {
            var remainingCount = CodeExitIndex - 1 - IdentifierIndex;
            return Context.Tokens.GetRange(IdentifierIndex + 1, remainingCount);
        }

        public void Reset(Context context)
        {
            Context = context;
            StartIndex = context.Index + 1;
            CodeExitIndex = -1;
            Length = 0;
            IdentifierIndex = -1;
            RawTextCount = 0;
        }
    }

    private static void ParseCodeBlock(Context context, CodeBlockContext codeBlockContext)
    {
        codeBlockContext.Reset(context);
        while (context.MoveNext())
        {
            codeBlockContext.RawTextCount += context.Current.Type == TokenType.Raw ? 1 : 0;

            if (context.Current.Type == TokenType.CodeExit)
            {
                codeBlockContext.CodeExitIndex = context.Index;
                break;
            }

            codeBlockContext.Length++;

            if (context.Current.Type == TokenType.CodeEnter)
            {
                throw context.Failure("Nested code blocks are not allowed");
            }

            if (codeBlockContext.IdentifierIndex < 0 && context.Current.Type == TokenType.Raw)
            {
                codeBlockContext.IdentifierIndex = context.Index;
            }
        }

        if (codeBlockContext.IdentifierIndex < 0)
        {
            throw context.Failure(codeBlockContext.StartIndex, "Code block is empty");
        }

        if (codeBlockContext.CodeExitIndex < 0)
        {
            throw context.Failure(codeBlockContext.Length - 1, "Code exit token not found");
        }
    }
}
