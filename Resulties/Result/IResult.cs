namespace Resulties.Result;

public interface IResult<out TResult> {
    public static abstract TResult Success();
    public static abstract TResult Success<TValue>(TValue value);
    public static abstract TResult Error(Error error);
    public static abstract TResult Async(Task task);
}