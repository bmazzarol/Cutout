using System.CodeDom.Compiler;

namespace Cutout.Extensions;

internal static class IndentedTextWriterExtensions
{
    internal readonly ref struct IndentationDisposable(IndentedTextWriter writer)
    {
        public void Dispose()
        {
            writer.Indent--;
        }
    }

    /// <summary>
    /// Indent the writer, returning a disposable object that will unindent the writer when disposed
    /// </summary>
    /// <param name="writer">writer to indent</param>
    /// <returns>disposable object that will unindent the writer when disposed</returns>
    public static IndentationDisposable Indent(this IndentedTextWriter writer)
    {
        writer.Indent++;
        return new IndentationDisposable(writer);
    }
}
