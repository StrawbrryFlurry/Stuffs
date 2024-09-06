
namespace Pippy;

public readonly ref struct Pipeable<T>(T value) {
    public T Value { get; } = value;
    public static implicit operator Pipeable<T>(T value) => new(value);
}

public interface IPipeTransform<in TSelf, T, TTo> where TSelf : IPipeTransform<TSelf, T, TTo>, allows ref struct {
    public static abstract Pipeable<TTo> operator |(Pipeable<T> source, TSelf self);
}

public static class Pipes {
    public static WhereEnumerablePipe<T> Where<T>(Func<T, bool> predicate) => new(predicate);
    public static TakeEnumerablePipe<T> Take<T>(int count) => new(count);
    public static SelectEnumerable<T, TTo> Select<T, TTo>(Func<T, TTo> selector) => new(selector);
    public static ToArrayEnumerablePipe<T> ToArray<T>() => default;
    
    public static SelectPipe<T, TTo> To<T, TTo>(Func<T, TTo> selector) => new(selector);
    public static UnwrapPipe<T> Unwrap<T>() => default;
}

public readonly ref struct WhereEnumerablePipe<T>(Func<T, bool> predicate) : IPipeTransform<WhereEnumerablePipe<T>, IEnumerable<T>, IEnumerable<T>> {
    private readonly Func<T, bool> _predicate = predicate;

    public static Pipeable<IEnumerable<T>> operator |(Pipeable<IEnumerable<T>> source, WhereEnumerablePipe<T> self) {
        return new Pipeable<IEnumerable<T>>(source.Value.Where(self._predicate));
    }
}

public readonly ref struct TakeEnumerablePipe<T>(int count) : IPipeTransform<TakeEnumerablePipe<T>, IEnumerable<T>, IEnumerable<T>> {
    private readonly int _count = count;

    public static Pipeable<IEnumerable<T>> operator |(Pipeable<IEnumerable<T>> source, TakeEnumerablePipe<T> self) {
        return new Pipeable<IEnumerable<T>>(source.Value.Take(self._count));
    }
}

public readonly ref struct SelectEnumerable<T, TTo>(Func<T, TTo> selector) : IPipeTransform<SelectEnumerable<T, TTo>, IEnumerable<T>, IEnumerable<TTo>> {
    private readonly Func<T, TTo> _selector = selector;

    public static Pipeable<IEnumerable<TTo>> operator |(Pipeable<IEnumerable<T>> source, SelectEnumerable<T, TTo> self) {
        return new Pipeable<IEnumerable<TTo>>(source.Value.Select(self._selector));
    }
}

public readonly ref struct ToArrayEnumerablePipe<T> {
    public static T[] operator |(Pipeable<IEnumerable<T>> source, ToArrayEnumerablePipe<T> self) {
        return source.Value.ToArray();
    }
}

public readonly ref struct SelectPipe<T, TTo>(Func<T, TTo> selector) : IPipeTransform<SelectPipe<T, TTo>, T, TTo> {
    private readonly Func<T, TTo> _selector = selector;
    
    public static Pipeable<TTo> operator |(Pipeable<T> source, SelectPipe<T, TTo> self) {
        var v = source.Value;
        return new Pipeable<TTo>(self._selector(v)); 
    }
}

public readonly ref struct UnwrapPipe<T> {
    public static T operator |(Pipeable<T> source, UnwrapPipe<T> self) {
        return source.Value;
    }
}