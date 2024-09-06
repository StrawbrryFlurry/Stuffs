using System.Runtime.CompilerServices;

namespace Resulties.Result;

public struct AsyncResultMethodBuilder {
    private AsyncResultMethodBuilderCore<Result>.ResultWrapper _result;

    public Result Task {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _result.TaskValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncResultMethodBuilder Create() {
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult() {
        AsyncResultMethodBuilderCore<Result>.SetResult(ref _result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetException(Exception exception) {
        AsyncResultMethodBuilderCore<Result>.SetException(ref _result, exception);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine
    ) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
        AsyncResultMethodBuilderCore<Result>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine
    ) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
        AsyncResultMethodBuilderCore<Result>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
        AsyncResultMethodBuilderCore<Result>.Start(ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
}