using Scriban.Parsing;

namespace Cutout.Exceptions;

internal sealed class ParseException : Exception
{
    public Token Token { get; }
    public string Value { get; }

    internal ParseException(Token token, string value, string message)
        : base(BuildMessage(token, value, message))
    {
        Token = token;
        Value = value;
    }

    private static string BuildMessage(Token token, string value, string message)
    {
        var tokenType = token.Type.ToString();
        var lineNumber = token.Start.Line;
        var columnNumber = token.Start.Column;

        return $"Parse error at {lineNumber}:{columnNumber} ({tokenType}): {message} (value: '{value}')";
    }
}
