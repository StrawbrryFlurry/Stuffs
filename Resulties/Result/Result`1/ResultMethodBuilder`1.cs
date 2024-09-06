using System.Runtime.CompilerServices;

namespace Resulties.Result;

public struct AsyncResultMethodBuilder<TResult> {
    private AsyncResultMethodBuilderCore<Result<TResult>>.ResultWrapper _result;

    public Result<TResult> Task {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _result.TaskValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncResultMethodBuilder<TResult> Create() {
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetResult(TResult result) {
        AsyncResultMethodBuilderCore<Result<TResult>>.SetResult(result, ref _result);
    }

    public void SetException(Exception exception) {
        AsyncResultMethodBuilderCore<Result<TResult>>.SetException(ref _result, exception);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine
    ) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
        AsyncResultMethodBuilderCore<Result<TResult>>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine
    ) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
        AsyncResultMethodBuilderCore<Result<TResult>>.AwaitOnCompleted(ref awaiter, ref stateMachine, ref _result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
        AsyncResultMethodBuilderCore<Result<TResult>>.Start(ref stateMachine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
}