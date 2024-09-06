using System.Runtime.CompilerServices;

namespace Resulties.TaskResult;

public ref struct AsyncTaskResultMethodBuilder<TValue> {
    private TaskResult<TValue>? _result;
    private AsyncTaskMethodBuilder<TValue> _builder = AsyncTaskMethodBuilder<TValue>.Create();

    public AsyncTaskResultMethodBuilder() { }

    public TaskResult<TValue> Task {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return _result ??= new TaskResult<TValue>(_builder.Task); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncTaskResultMethodBuilder<TValue> Create() {
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult(TValue value) {
        Task.Complete();
        _builder.SetResult(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception) {
        if (exception is AccessValueOnFailureResultException resultFailure) {
            Task.CompleteWithError(resultFailure.Error);
            _builder.SetResult(default!); // We treat the task to be completed successfully, even though it is a failure - the awaiter will handle failure results accordingly
            return;
        }

        Task.CompleteWithError(new Error("UnhandledException", exception.Message, exception));
        _builder.SetResult(default!);
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