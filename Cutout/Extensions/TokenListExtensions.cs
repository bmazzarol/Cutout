using System.Text;

namespace Cutout;

internal static class TokenListExtensions
{
    internal static string ToString(this TokenList list, string template)
    {
        if (list.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var token in list)
        {
            if (token.Type == TokenType.Eof)
            {
                break;
            }

            foreach (var c in token.ToSpan(template))
            {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }
}
