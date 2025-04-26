using Fluidic.Extensions;
using Scriban.Parsing;

namespace Fluidic.Parser;

internal static partial class TemplateParser
{
    private static Syntax.VarStatement ParseVarStatement(
        ReadOnlySpan<Token> tokens,
        ReadOnlySpan<char> template,
        ref int index
    )
    {
        ExtractCodeTokens(tokens, template, ref index, out var start, out var end);
        TrySkipWhitespace(tokens, template, ref index);
        return new Syntax.VarStatement(start.ToSpan(template, end: end).ToString());
    }
}
