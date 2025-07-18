using System.Runtime.CompilerServices;

namespace ArrayBuilder;

[InterpolatedStringHandler]
public ref struct FormattableSyntaxWritable {
    private ref ValueSyntaxWriter _writer;

    public int Length => _writer.Length;

    public FormattableSyntaxWritable(int literalLength, int formattedCount, [CallerMemberName] string memberName = "") {
        _writer = new ValueSyntaxWriter();
    }

    public void AppendLiteral(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted(string s) {
        _writer.Write(s);
    }

    public readonly void CopyToAndDispose(Span<char> buffer) {
        _writer.CopyTo(buffer);
        _writer.Dispose();
    }
}