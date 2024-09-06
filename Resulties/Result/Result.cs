using System.Runtime.CompilerServices;

namespace Resulties.Result;

[AsyncMethodBuilder(typeof(AsyncResultMethodBuilder))]
public readonly struct Result : IResult<Result> {
    public static Result Success { get; } = new();

    public bool IsSuccess { get; }
    public Error Error { get; }

    internal readonly Task? AsyncCompletionSource;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result() {
        IsSuccess = true;
    }

    public Result(in Error error) {
        Error = error;
        IsSuccess = false;
    }

    public Result(in Task asyncCompletionSource) {
        AsyncCompletionSource = asyncCompletionSource;
        IsSuccess = true;
    }

    public static implicit operator Result(Error error) {
        return new Result(error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultAwaiter GetAwaiter() {
        return new ResultAwaiter(in this);
    }

    static Result IResult<Result>.Success() {
        return Success;
    }

    static Result IResult<Result>.Success<TValue>(TValue value) {
        return Success;
    }

    static Result IResult<Result>.Error(Error error) {
        return error;
    }

    public static Result Async(Task task) {
        return new Result(task);
    }
}