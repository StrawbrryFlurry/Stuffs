using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Resulties.TaskResult;

public readonly struct TaskResultAwaiter<TResult> : ICriticalNotifyCompletion {
    private readonly TaskResult<TResult> _result;
    public bool IsCompleted => _awaiter.IsCompleted;

    private readonly TaskAwaiter<TResult> _awaiter;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TaskResultAwaiter(in TaskResult<TResult> result) {
        _result = result;
        TaskAwaiterInternal.InitTaskAwaiter(ref _awaiter, result.Task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult GetResult() {
        var result = _awaiter.GetResult();
        if (_result.IsSuccess) {
            return result;
        }

        NotifyAwaiterResultFailed();
        return default;
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
        private readonly TaskResult<TResult> _result;

        public AsResultTaskResultAwaitable(TaskResult<TResult> result) {
            _result = result;
        }

        public AsResultResultAwaiter GetAwaiter() {
            return new AsResultResultAwaiter(_result);
        }

        public readonly struct AsResultResultAwaiter : ICriticalNotifyCompletion {
            private readonly TaskResult<TResult> _result;
            private readonly TaskAwaiter<TResult> _awaiter;

            public bool IsCompleted => _awaiter.IsCompleted;

            public AsResultResultAwaiter(TaskResult<TResult> result) {
                _result = result;
                TaskAwaiterInternal.InitTaskAwaiter(ref _awaiter, result.Task);
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

    private static class TaskAwaiterInternal {
        // Unsafe accessors do not yet allow us to call methods with generic parameters,
        // so we use this hack to initialize the field of the struct with the task result
        // The dotnet team already relies on the layout of the TaskAwaiter struct, so it 
        // should be relatively safe to do this without worrying about the fields changing
        // in the future.
        public static unsafe void InitTaskAwaiter(ref TaskAwaiter<TResult> field, Task<TResult> result) {
            field = default;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            fixed (TaskAwaiter<TResult>* ptr = &field) {
                Unsafe.Write(ptr, result);
            }
#if DEBUG
            var setValue = field.GetType().GetField("m_task")!.GetValue(field);
            Debug.Assert(setValue == result, "TaskAwaiter<TResult> m_task field was not set correctly");
#endif
        }
    }
}

public static class TaskResultAwaiterT1Extensions {
    public static TaskResultAwaiter<TValue>.AsResultTaskResultAwaitable Result<TValue>(this TaskResult<TValue> result) {
        return new TaskResultAwaiter<TValue>.AsResultTaskResultAwaitable(result);
    }
}