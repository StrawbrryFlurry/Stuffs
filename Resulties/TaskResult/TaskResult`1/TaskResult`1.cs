using System.Runtime.CompilerServices;

namespace Resulties.TaskResult;

[AsyncMethodBuilder(typeof(AsyncTaskResultMethodBuilder<>))]
public sealed class TaskResult<TValue> {
    public bool IsCompleted => Task.IsCompleted;

    public bool IsSuccess { get; private set; }
    public Error Error { get; private set; }

    public Task<TValue> Task { get; }

    public TaskResult(Task<TValue> awaiterTask) {
        Task = awaiterTask;
    }

    public TaskResultAwaiter<TValue> GetAwaiter() {
        return new TaskResultAwaiter<TValue>(this);
    }

    public void Complete() {
        IsSuccess = true;
        Error = default;
    }

    public void CompleteWithError(in Error error) {
        IsSuccess = false;
        Error = error;
    }
}