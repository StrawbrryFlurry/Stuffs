using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ArrayBuilder;

public static class ArrayBuilder {
    public const int LargeElementInitSize = 64;
    public const int InitSize = 256;
}

public ref partial struct ArrayBuilder<TElement> {
    private const int DefaultCapacity = 4;

    private Span<TElement> _elements;
    private TElement[]? _rentedArray;
    private int _count = 0;

    public static ArrayBuilder<TElement> Empty => new();

    public Span<TElement> Elements {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements[.._count];
    }

    public Span<TElement> UnsafeBuffer {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements;
    }

    public ArrayBuilder() { }

    public ArrayBuilder(in Span<TElement> initialBuffer) {
        _elements = initialBuffer;
    }

    internal ArrayBuilder(int initialCapacity) {
        if (initialCapacity < 0) {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        var allocatedCapacity = GetCapacityForSize(initialCapacity);
        var array = ArrayPool<TElement>.Shared.Rent(allocatedCapacity);
        _elements = _rentedArray = array;
    }

    private static int GetCapacityForSize(int count) {
        if (count < 0) {
            return 0;
        }

        // Prefer common array sizes
        return count switch {
            <= DefaultCapacity => DefaultCapacity,
            <= 8 => 8,
            <= 16 => 16,
            <= 32 => 32,
            <= 64 => 64,
            <= 128 => 128,
            <= 256 => 256,
            <= 512 => 512,
            <= 1024 => 1024,
            <= 2048 => 2048,
            _ => count
        };
    }

    public void AddRange(in ReadOnlySpan<TElement> elements) {
        var count = _count;
        var newCount = count + elements.Length;
        var buffer = _elements;
        if ((uint)newCount < (uint)buffer.Length) {
            elements.CopyTo(_elements[count..]);
            _count = newCount;
            return;
        }

        Grow(newCount);

        elements.CopyTo(_elements[count..]);
        _count = newCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in TElement element) {
        var count = _count;
        var buffer = _elements;
        if ((uint)count < (uint)buffer.Length) {
            buffer[count] = element;
            _count = count + 1;
            return;
        }

        Grow(count + 1);
        _elements[count] = element;
        _count = count + 1;
    }

    private void Grow(int requiredCapacity) {
        Debug.Assert(_count < requiredCapacity);

        var buffer = _elements;
        var len = buffer.Length;
        var newCapacity = Math.Max(len == 0 ? DefaultCapacity : len * 2, requiredCapacity + 1);

        var resizedElements = ArrayPool<TElement>.Shared.Rent(newCapacity);
        buffer[.._count].CopyTo(resizedElements);
        if (_rentedArray is not null) {
            ArrayPool<TElement>.Shared.Return(_rentedArray);
        }

        _elements = _rentedArray = resizedElements;
    }

    public TElement[] ToArrayAndFree() {
        var array = new TElement[_count];

        Elements.CopyTo(array);
        Dispose();

        return array;
    }

    public ImmutableArray<TElement> ToImmutableArrayAndFree() {
        // The ImmutableArray constructor will create a new array from the span,
        // so we can give it a cut-down version of the array we rented, avoiding
        // allocating the correctly sized array twice.
        var array = ImmutableArray.Create(Elements);
        Dispose();
        return array;
    }

    public Span<TElement> ToSpanAndFree() {
        Span<TElement> backingArray = new TElement[_count];

        Elements.CopyTo(backingArray);
        Dispose();

        return backingArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TElement> ToReadOnlySpanAndFree() {
        return new ReadOnlySpan<TElement>(ToArrayAndFree());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TemporarySpan<TElement> AsRefForfeitOwnership() {
        return new TemporarySpan<TElement>(Elements, _rentedArray);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TElement> DangerousAsSpanWithoutOwnership() {
        return Elements;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TElement> DangerousAsWriteableSpanWithoutOwnership() {
        return Elements;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        var rentedArray = _rentedArray;
        if (rentedArray is not null) {
            ArrayPool<TElement>.Shared.Return(rentedArray);
        }

        this = default;
    }
}

public static class ArrayBuilderExtensions {
    public static int IndexOf<T>(this global::ArrayBuilder.ArrayBuilder<T> builder, T value) where T : IEquatable<T> {
        return builder.DangerousAsSpanWithoutOwnership().IndexOf(value);
    }

    public static bool Contains<T>(this global::ArrayBuilder.ArrayBuilder<T> builder, T value) where T : IEquatable<T> {
        return builder.IndexOf(value) is not -1;
    }

    public static ref T? TryGet<T>(this global::ArrayBuilder.ArrayBuilder<T> builder, T element) where T : IEquatable<T> {
        var index = builder.IndexOf(element);
        if (index is not -1) {
            return ref builder.DangerousAsWriteableSpanWithoutOwnership()[index]!;
        }

        return ref Unsafe.NullRef<T>()!;
    }

    public static unsafe string ToStringAndFree(this global::ArrayBuilder.ArrayBuilder<char> builder) {
        var chars = builder.DangerousAsSpanWithoutOwnership();

        // The string ctor should copy all chars to its own buffer and
        // allow us to free the rented array, without breaking the string.
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.cs,151
        string str;
        fixed (char* p = chars) {
            str = new string(p, 0, chars.Length);
        }

        builder.Dispose();
        return str;
    }
}