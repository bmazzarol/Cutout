namespace Cutout.Extensions;

internal static class TokenListExtensions
{
    internal static ReadOnlySpan<char> ToSpan(this TokenList list, in ReadOnlySpan<char> template)
    {
        if (list.Count == 0)
        {
            return ReadOnlySpan<char>.Empty;
        }

        var start = list[0];
        var end = list[list.Count - 1];
        return start.ToSpan(in template, end);
    }
}
