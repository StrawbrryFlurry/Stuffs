using System.Runtime.CompilerServices;

namespace Resulties.Result;

[AsyncMethodBuilder(typeof(AsyncResultMethodBuilder<>))]
public readonly struct Result<TValue> : IResult<Result<TValue>> {
    // ReSharper disable once InconsistentNaming
    internal readonly TValue? _value;

    public bool IsSuccess {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    public TValue Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsSuccess
            ? _value!
            : throw new AccessValueOnFailureResultException(Error);
    }

    internal readonly Task? AsyncCompletionSource = null;

    public Error Error { get; }

    public Result() {
        IsSuccess = true;
        _value = default;
    }

    internal Result(Task asyncCompletionSource) {
        IsSuccess = true;
        AsyncCompletionSource = asyncCompletionSource;
    }

    public Result(in TValue value) {
        IsSuccess = true;
        _value = value;
    }

    public Result(in Error error) {
        IsSuccess = false;
        Error = error;
    }

    public static implicit operator Result<TValue>(TValue value) {
        return new Result<TValue>(value);
    }

    public static implicit operator TValue(Result<TValue> result) {
        return result.Value;
    }

    public static implicit operator Result<TValue>(Error error) {
        return new Result<TValue>(error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultAwaiter<TValue> GetAwaiter() {
        return new ResultAwaiter<TValue>(in this);
    }

    public static Result<TValue> Success() {
        return new Result<TValue>();
    }

    public static Result<TValue> Success<T>(T value) {
        return new Result<TValue>(Unsafe.As<T, TValue>(ref value));
    }

    static Result<TValue> IResult<Result<TValue>>.Error(Error error) {
        return new Result<TValue>(error);
    }

    public static Result<TValue> Async(Task task) {
        return new Result<TValue>(task);
    }
}