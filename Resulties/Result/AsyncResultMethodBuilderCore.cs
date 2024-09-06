using System.Runtime.CompilerServices;

namespace Resulties.Result;

internal static class AsyncResultMethodBuilderCore<TResult> where TResult : IResult<TResult> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetResult<TValue>(in TValue value, ref ResultWrapper resultField) {
        resultField = resultField.ToSuccessWith(value);
        resultField._box?.Complete();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetResult(ref ResultWrapper resultField) {
        resultField = resultField.ToSuccess();
        resultField._box?.Complete();
    }

    public static void SetException(ref ResultWrapper resultField, Exception exception) {
        if (exception is AccessValueOnFailureResultException resultFailure) {
            resultField = resultField.ToError(resultFailure.Error);
            resultField._box?.Complete();
            return;
        }

        resultField = resultField.ToError(new Error("Error.SetException", exception.Message, exception));
        resultField._box?.Complete();
    }

    public static void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine,
        ref ResultWrapper resultField
    ) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
        if (resultField._box is AsyncStateMachineBox<TStateMachine> box) {
            awaiter.OnCompleted(box.MoveNext);
            return;
        }

        box = new AsyncStateMachineBox<TStateMachine>(stateMachine);
        awaiter.OnCompleted(box.MoveNext);
        resultField = new ResultWrapper(box);
    }

    public static void Start<TStateMachine>(
        ref TStateMachine stateMachine
    ) where TStateMachine : IAsyncStateMachine {
        stateMachine.MoveNext();
    }

    private sealed class AsyncStateMachineBox<TStateMachine> : IStateMachineBox where TStateMachine : IAsyncStateMachine {
        private readonly TStateMachine _stateMachine;
        private readonly TaskCompletionSource _source;

        private Action? _moveNext;

        Task IStateMachineBox.Task {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.Task;
        }

        public AsyncStateMachineBox(TStateMachine stateMachine) {
            _stateMachine = stateMachine;
            _source = new TaskCompletionSource();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveNext() {
            _moveNext ??= _stateMachine.MoveNext;
            _moveNext();
        }

        public void Complete() {
            _source.SetResult();
        }
    }

    internal interface IStateMachineBox {
        internal void MoveNext();
        internal Task Task { get; }
        internal void Complete();
    }

    internal struct ResultWrapper {
        internal TResult TaskValue;
        internal IStateMachineBox? _box;

        public ResultWrapper() {
            TaskValue = TResult.Async(Task.CompletedTask);
        }

        internal ResultWrapper(IStateMachineBox box) {
            _box = box;
            TaskValue = TResult.Async(_box.Task);
        }

        public ResultWrapper ToSuccess() {
            TaskValue = TResult.Success();
            return this;
        }

        public ResultWrapper ToSuccessWith<TValue>(in TValue value) {
            TaskValue = TResult.Success(value);
            return this;
        }

        public ResultWrapper ToError(Error error) {
            TaskValue = TResult.Error(error);
            return this;
        }
    }
}