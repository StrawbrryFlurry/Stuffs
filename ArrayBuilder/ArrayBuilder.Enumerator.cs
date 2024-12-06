using System.Runtime.CompilerServices;

namespace ArrayBuilder;

public ref partial struct ArrayBuilder<TElement> {
  public Enumerator GetEnumerator() {
    return new Enumerator(_elements, _count);
  }

  public ref struct Enumerator {
    private readonly Span<TElement> _span;
    private readonly int _actualLength;
    private int _index;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Enumerator(Span<TElement> span, int actualLength) {
      _span = span;
      _actualLength = actualLength;
      _index = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() {
      var idx = _index + 1;
      if (idx >= _actualLength) {
        return false;
      }

      _index = idx;
      return true;
    }

    public readonly ref readonly TElement Current {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ref _span[_index];
    }
  }
}