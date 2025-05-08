using Scriban.Parsing;

namespace Cutout.Parser;

internal static partial class TemplateParser
{
    internal interface IParseContext
    {
        bool ShouldBreak(ReadOnlySpan<Token> tokens, ReadOnlySpan<char> template, ref int index);
        string MessageOnNoBreak { get; }

        bool ShouldSkipLeadingNewline { get; }
        bool ShouldSkipTrailingNewline { get; }
    }
}
