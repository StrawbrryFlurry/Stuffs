using System.Runtime.CompilerServices;

namespace Resulties;

internal sealed class AccessValueOnFailureResultException : InvalidOperationException {
    internal readonly Error _error;

    public ref readonly Error Error {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _error;
    }

    public AccessValueOnFailureResultException(in Error error) : base(error.Message, error.Cause) {
        _error = error;
    }
}