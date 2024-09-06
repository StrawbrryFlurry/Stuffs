using System.Runtime.CompilerServices;

namespace Resulties.TaskResult;

[AsyncMethodBuilder(typeof(AsyncTaskResultMethodBuilder))]
public sealed class TaskResult {
    public bool IsCompleted => Task.IsCompleted;

    public bool IsSuccess { get; private set; }
    public Error Error { get; private set; }

    public Task Task { get; }

    public TaskResult(Task awaiterTask) {
        Task = awaiterTask;
    }

    public TaskResultAwaiter GetAwaiter() {
        return new TaskResultAwaiter(this);
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