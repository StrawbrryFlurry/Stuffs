using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ArrayBuilder;

public ref partial struct ArrayBuilder<TElement> {
  private Span<TElement> _elements;
  
  private int _count = 0;

  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _count;
  }
  
  private Span<TElement> SlicedElements {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => _elements[.._count];
  }

  public static ArrayBuilder<TElement> Empty => default;

  public ArrayBuilder() {
    _elements = default;
  }

  /// <summary>
  /// </summary>
  /// <param name="initialCapacity"></param>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  public ArrayBuilder(uint initialCapacity) {
    Grow(initialCapacity);
  }

  /// <summary>
  /// Returns a new <see cref="Array"/> containing all elements of the builder.
  /// The builder cannot be used after this operation.
  /// </summary>
  /// <returns>A new array containing all the elements of the builder</returns>
  [SkipLocalsInit]
  public TElement[] ToArrayAndFree() {
    var array = new TElement[_count];

    SlicedElements.CopyTo(array);

    return array;
  }

  /// <summary>
  /// Returns a new <see cref="ImmutableArray{T}"/> containing all elements of the builder.
  /// The builder cannot be used after this operation.
  /// </summary>
  /// <returns>A new array containing all the elements of the builder</returns>
  public ImmutableArray<TElement> ToImmutableArrayAndFree() {
    // The ImmutableArray constructor will create a new array from the span,
    // so we can give it a cut-down version of the array we rented, avoiding
    // allocating the correctly sized array twice.
    var array = ImmutableArray.Create(SlicedElements);
    return array;
  }

  /// <summary>
  /// Returns a new <see cref="Span{T}"/> containing all elements of the builder.
  /// The builder cannot be used after this operation.
  /// </summary>
  /// <returns>A new span containing all the elements of the builder</returns>
  [SkipLocalsInit]
  public Span<TElement> ToSpanAndFree() {
    Span<TElement> backingArray = new TElement[_count];

    SlicedElements.CopyTo(backingArray);

    return backingArray;
  }
  
  public void CopyTo(Span<TElement> span) {
    SlicedElements.CopyTo(span);
  }

  /// <summary>
  /// Returns a new <see cref="ReadOnlySpan{T}"/> containing all elements of the builder.
  /// The builder cannot be used after this operation.
  /// </summary>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<TElement> ToReadOnlySpanAndFree() {
    return new ReadOnlySpan<TElement>(ToArrayAndFree());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TemporarySpan<TElement> AsRefForfeitOwnership() {
    return new TemporarySpan<TElement>(SlicedElements);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UnsafeTempSpan<TElement> AsVeryUnsafeRefForfeitOwnership() {
    return new UnsafeTempSpan<TElement>(SlicedElements);
  }

  /// <summary>
  /// Returns a span that includes all elements in the builder, useful for
  /// iterating over the elements or copying them to another span.
  /// Consumers DO NOT own the returned span and MUST NOT use it after
  /// this builder instance has been used again, mutate it,
  /// pass it to another method or otherwise leak it outside of it's owned context.
  /// The backing array of the span is still used by the builder and will be changed
  /// / updated it for future operations.
  /// </summary>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<TElement> DangerousAsSpanWithoutOwnership() {
    return SlicedElements;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal Span<TElement> DangerousAsWriteableSpanWithoutOwnership() {
    return SlicedElements;
  }
  
  public void AddRange(IEnumerable<TElement> elements) {
    foreach (var element in elements) {
      Add(element);
    }
  }

  public void AddRange(ReadOnlySpan<TElement> elements) {
    var newCount = (_count + elements.Length);
    var elementsRef = _elements;
    if (newCount <= elementsRef.Length) {
      elements.CopyTo(elementsRef[_count..]);
      _count = newCount;
      return;
    }

    Grow((uint)newCount);

    elements.CopyTo(_elements[_count..]); // Might be a new array
    _count = newCount;
  }

  public void Add(TElement element) {
    var count = _count;
    var elementsRef = _elements;
    if (count < elementsRef.Length) {
      elementsRef[_count] = element;
      _count++;
      return;
    }

    Grow((uint)_count + 1);
    _elements[_count] = element;
    _count++;
  }

  public void GrowTo(uint capacity) {
    Grow(capacity);
  }

  [SkipLocalsInit]
  private void Grow(uint size) {
    var oldBufferRef = _currentBuffer;
    var oldElements = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayBuffer128, TElement>(ref oldBufferRef), _count);
    Span<TElement> newBuffer;
    switch (size) {
      case <= 4:
        ref var b4 = ref Unsafe.As<ArrayBuffer128, ArrayBuffer4>(ref _currentBuffer);
        b4 = new ArrayBuffer4();
        newBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayBuffer4, TElement>(ref b4), 4) ;
        break;
      case <= 16:
        ref var b16 = ref Unsafe.As<ArrayBuffer128, ArrayBuffer16>(ref _currentBuffer);
        b16 = new ArrayBuffer16();
        newBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayBuffer16, TElement>(ref b16), 16);
        break;
      case <= 32:
        ref var b32 = ref Unsafe.As<ArrayBuffer128, ArrayBuffer32>(ref _currentBuffer);
        b32 = new ArrayBuffer32();
        newBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayBuffer32, TElement>(ref b32), 32);
        break;
      case <= 64:
        ref var b64 = ref Unsafe.As<ArrayBuffer128, ArrayBuffer64>(ref _currentBuffer);
        b64 = new ArrayBuffer64();
        newBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayBuffer64, TElement>(ref b64), 64);
        break;
      case <= 128 when !typeof(TElement).IsValueType:
        ref var b128 = ref Unsafe.As<ArrayBuffer128, ArrayBuffer128>(ref _currentBuffer);
        b128 = new ArrayBuffer128();
        newBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayBuffer128, TElement>(ref b128), 128);
        break;
      default:
        var bufferSize = BitOperations.RoundUpToPowerOf2(size);
        var dynamicBuffer = new TElement[bufferSize];
        newBuffer = dynamicBuffer.AsSpan();
        break;
    }
    
    if (_count != 0) {
      oldElements.CopyTo(newBuffer);
    }
    
    _elements = newBuffer;
  }

  private static void ClearBuffers() {
    // ReSharper disable SuspiciousTypeConversion.Global
    if (_currentBuffer is not NullBuffer) {
      _currentBuffer = Unsafe.As<NullBuffer, ArrayBuffer128>(ref _nullBufferInstance);
    }
  }
}
