namespace Cutout;

internal static partial class Parser
{
    private sealed class BlockContext
    {
        private Context Context { get; set; }
        public int StartIndex { get; private set; }
        public int ExitIndex { get; set; }
        public int Length { get; set; }
        public int RawTextCount { get; set; }
        public int IdentifierIndex { get; set; }

        public ReadOnlySpan<char> Identifier =>
            Context.Tokens[IdentifierIndex].ToSpan(Context.Template);
        public bool IsJustIdentifier => RawTextCount == 1;

        public BlockContext(Context context)
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
            var remainingCount = ExitIndex - 1 - IdentifierIndex;
            return Context.Tokens.GetRange(IdentifierIndex + 1, remainingCount);
        }

        public void Reset(Context context)
        {
            Context = context;
            StartIndex = context.Index + 1;
            ExitIndex = -1;
            Length = 0;
            IdentifierIndex = -1;
            RawTextCount = 0;
        }
    }

    private static void ParseBlock(Context context, BlockContext blockContext)
    {
        blockContext.Reset(context);
        while (context.MoveNext())
        {
            blockContext.RawTextCount += context.Current.Type == TokenType.Raw ? 1 : 0;

            if (context.Current.IsBlockExitToken())
            {
                var startBlockToken = context.Tokens[blockContext.StartIndex - 1];
                if (
                    startBlockToken.IsCodeBlockEnterToken()
                    && context.Current.IsRenderBlockExitToken()
                )
                {
                    throw context.Failure("Render block exit token cannot be used in code blocks");
                }
                if (
                    startBlockToken.IsRenderBlockEnterToken()
                    && context.Current.IsCodeBlockExitToken()
                )
                {
                    throw context.Failure("Code block exit token cannot be used in render blocks");
                }

                blockContext.ExitIndex = context.Index;
                break;
            }

            blockContext.Length++;

            if (context.Current.IsBlockEnterToken())
            {
                throw context.Failure("Nested code or render blocks are not allowed");
            }

            if (blockContext.IdentifierIndex < 0 && context.Current.Type == TokenType.Raw)
            {
                blockContext.IdentifierIndex = context.Index;
            }
        }

        if (blockContext.IdentifierIndex < 0)
        {
            throw context.Failure(blockContext.StartIndex, "Code block is empty");
        }

        if (blockContext.ExitIndex < 0)
        {
            throw context.Failure(blockContext.Length - 1, "Code exit token not found");
        }
    }
}
