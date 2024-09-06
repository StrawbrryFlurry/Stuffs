using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Resulties.Result;

public struct ResultAwaiter<TResult> : ICriticalNotifyCompletion {
    private readonly Result<TResult> _result;
    public bool IsCompleted => true;

    public ResultAwaiter(in Result<TResult> result) {
        _result = result;
    }

    public TResult GetResult() {
        if (_result.AsyncCompletionSource is { } cs) {
            cs.GetAwaiter().GetResult();
        }

        if (_result.IsSuccess) {
            return _result._value!;
        }

        NotifyAwaiterResultFailed();
        return default;
    }

    [DoesNotReturn]
    private void NotifyAwaiterResultFailed() {
        throw new AccessValueOnFailureResultException(_result.Error);
    }

    public void OnCompleted(Action continuation) {
        continuation();
    }

    public void UnsafeOnCompleted(Action continuation) {
        continuation();
    }
}