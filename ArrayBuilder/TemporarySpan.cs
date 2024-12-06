using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ArrayBuilder;


public readonly ref struct UnsafeTempSpan<T> {
  internal readonly ref T _reference;
  public readonly int Length;
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UnsafeTempSpan(ReadOnlySpan<T> span) {
    _reference = ref MemoryMarshal.GetReference(span);
    Length = span.Length;
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref T GetPinnableReference() {
    return ref _reference;
  }
}

/// <summary>
/// Wraps a <see cref="Span{T}"/>, giving
/// consumers temporary access to its contents. The Span
/// is by design volatile and is not meant to be used outside
/// of the current operation scope. Consumers
/// may copy or mutate the contents of the span, but
/// must not store a reference to the span itself. The
/// instance must be disposed when the consumer is done using it.
///<remarks>
/// Using the instance after it has been disposed may result in
/// undefined behavior.
/// 
/// Not disposing the instance will result in performance
/// issues and cause additional memory to be allocated,
/// which might not be freed.
/// </remarks>
/// </summary>
/// <typeparam name="T">The type of the Span's content</typeparam>
public ref struct TemporarySpan<T> {
  private readonly ReadOnlySpan<T> _span;
  private readonly Action? _dispose;
  
  public int Length => _span.Length;

  public TemporarySpan(ReadOnlySpan<T> span, Action dispose) {
    _span = span;
    _dispose = dispose;
  }

  public TemporarySpan(ReadOnlySpan<T> span) {
    _span = span;
  }
    
  public readonly ref readonly T this[int index] => ref _span[index];

  public static implicit operator TemporarySpan<T>(ReadOnlySpan<T> span) {
    return new TemporarySpan<T>(span);
  }

  public void Dispose() {
    _dispose?.Invoke();
  }

  public Enumerator GetEnumerator() {
    return new Enumerator(_span, _span.Length);
  }

  public ref struct Enumerator {
    private readonly ReadOnlySpan<T> _span;
    private readonly int _actualLength;
    private int _index;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Enumerator(ReadOnlySpan<T> span, int actualLength) {
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

    public readonly ref readonly T Current {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ref _span[_index];
    }
  }
}