using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Resulties.TaskResult;

public struct TaskResultAwaiter : ICriticalNotifyCompletion {
    private readonly TaskResult _result;
    public bool IsCompleted => _awaiter.IsCompleted;

    private TaskAwaiter _awaiter;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TaskResultAwaiter(in TaskResult result) {
        _result = result;
        _awaiter = New.TaskAwaiter(result.Task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetResult() {
        _awaiter.GetResult();
        if (_result.IsSuccess) {
            return;
        }

        NotifyAwaiterResultFailed();
    }

    [DoesNotReturn]
    private void NotifyAwaiterResultFailed() {
        throw new AccessValueOnFailureResultException(_result.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) {
        _awaiter.OnCompleted(continuation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) {
        _awaiter.OnCompleted(continuation);
    }

    public readonly struct AsResultTaskResultAwaitable {
        private readonly TaskResult _result;

        public AsResultTaskResultAwaitable(TaskResult result) {
            _result = result;
        }

        public AsResultResultAwaiter GetAwaiter() {
            return new AsResultResultAwaiter(_result);
        }

        public readonly struct AsResultResultAwaiter : ICriticalNotifyCompletion {
            private readonly TaskResult _result;
            private readonly TaskAwaiter _awaiter;

            public bool IsCompleted => _awaiter.IsCompleted;

            public AsResultResultAwaiter(TaskResult result) {
                _result = result;
                _awaiter = New.TaskAwaiter(result.Task);
            }

            public Result.Result GetResult() {
                try {
                    _awaiter.GetResult();
                }
                catch (Exception e) {
                    return new Result.Result(new Error("TaskResultAwaiter.GetResult", e.Message, e));
                }

                return _result.IsSuccess
                    ? new Result.Result()
                    : new Result.Result(_result.Error);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation) {
                _awaiter.OnCompleted(continuation);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsafeOnCompleted(Action continuation) {
                _awaiter.OnCompleted(continuation);
            }
        }
    }

    private static class New {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        public static extern TaskAwaiter TaskAwaiter(Task result);
    }
}

public static class TaskResultAwaiterExtensions {
    public static TaskResultAwaiter.AsResultTaskResultAwaitable Result(this TaskResult result) {
        return new TaskResultAwaiter.AsResultTaskResultAwaitable(result);
    }
}