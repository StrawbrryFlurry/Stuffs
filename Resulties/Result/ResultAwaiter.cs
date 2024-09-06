using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Resulties.Result;

public struct ResultAwaiter : ICriticalNotifyCompletion {
    private readonly Result _result;

    public bool IsCompleted => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultAwaiter(in Result result) {
        _result = result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetResult() {
        if (_result.AsyncCompletionSource is { } cs) {
            cs.GetAwaiter().GetResult();
        }

        if (_result.IsSuccess) {
            return;
        }

        NotifyAwaiterResultFailed(); // Allow method inlining
    }

    [DoesNotReturn]
    private void NotifyAwaiterResultFailed() {
        throw new AccessValueOnFailureResultException(_result.Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) {
        continuation();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) {
        continuation();
    }
}