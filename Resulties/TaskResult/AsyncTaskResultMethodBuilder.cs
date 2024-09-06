using System.Runtime.CompilerServices;

namespace Resulties.TaskResult;

public struct AsyncTaskResultMethodBuilder {
    private TaskResult? _result;
    private AsyncTaskMethodBuilder _builder = AsyncTaskMethodBuilder.Create();

    public AsyncTaskResultMethodBuilder() { }

    public TaskResult Task {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return _result ??= new TaskResult(_builder.Task); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncTaskResultMethodBuilder Create() {
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult() {
        Task.Complete();
        _builder.SetResult();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception) {
        if (exception is AccessValueOnFailureResultException resultFailure) {
            Task.CompleteWithError(resultFailure.Error);
            _builder.SetResult(); // We treat the task to be completed successfully, even though it is a failure - the awaiter will handle failure results accordingly
            return;
        }

        Task.CompleteWithError(new Error("UnhandledException", exception.Message, exception));
        _builder.SetResult();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine
    ) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
        _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine
    ) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
        _builder.Start(ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStateMachine(IAsyncStateMachine stateMachine) {
        _builder.SetStateMachine(stateMachine);
    }
}