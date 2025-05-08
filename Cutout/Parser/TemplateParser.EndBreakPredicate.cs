using Cutout.Extensions;
using Scriban.Parsing;

namespace Cutout.Parser;

internal static partial class TemplateParser
{
    private sealed class EndParseContext : IParseContext
    {
        private EndParseContext(bool shouldSkipLeadingNewline, bool shouldSkipTrailingNewline)
        {
            ShouldSkipLeadingNewline = shouldSkipLeadingNewline;
            ShouldSkipTrailingNewline = shouldSkipTrailingNewline;
        }

        public static EndParseContext Instance { get; } =
            new(shouldSkipLeadingNewline: true, shouldSkipTrailingNewline: true);

        public static EndParseContext InstanceWithSkipLeadingNewline { get; } =
            new(shouldSkipLeadingNewline: true, shouldSkipTrailingNewline: false);

        public bool ShouldBreak(
            ReadOnlySpan<Token> tokens,
            ReadOnlySpan<char> template,
            ref int index
        )
        {
            var current = tokens[index];
            return current.ToSpan(template).SequenceEqual(Identifiers.End);
        }

        public string MessageOnNoBreak => "end not found";
        public bool ShouldSkipLeadingNewline { get; }
        public bool ShouldSkipTrailingNewline { get; }
    }
}
