namespace Resulties;

public readonly struct Error : IEquatable<Error> {
    public string Code { get; }
    public string Message { get; }
    public Exception? Cause { get; }

    public Error(string code, string message, Exception? cause = null) {
        Code = code;
        Message = message;
        Cause = cause;
    }

    public static bool operator ==(Error? a, Error? b) {
        if (a is null && b is null) {
            return true;
        }

        if (a is null || b is null) {
            return false;
        }

        return a.Value.Equals(b.Value);
    }

    public static bool operator !=(Error? a, Error? b) {
        return !(a == b);
    }

    public bool Equals(Error? other) {
        return other is not null && Equals(other.Value);
    }

    public bool Equals(Error other) {
        return Code == other.Code && Message == other.Message && Equals(Cause, other.Cause);
    }

    public override bool Equals(object? obj) {
        return obj is Error other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Code, Message, Cause);
    }

    public override string ToString() {
        if (this == default) {
            return "No error";
        }

        return Cause is not null
            ? $"Code: {Code}, Message: {Message}, Cause: {Cause.Message}"
            : $"Code: {Code}, Message: {Message}";
    }
}