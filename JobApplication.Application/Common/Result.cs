namespace JobApplication.Application.Common
{
    public enum ErrorKind { Validation, NotFound, Forbidden }

    /// <summary>
    /// Small wrapper so services can return "success + value" or "failure + error messages"
    /// without throwing exceptions for normal failures (wrong password, email already used...).
    /// </summary>
    public class Result<T>
    {
        public bool Succeeded { get; }
        public T? Value { get; }
        public IReadOnlyList<string> Errors { get; }
        public ErrorKind Kind { get; }

        private Result(bool succeeded, T? value, IEnumerable<string> errors, ErrorKind kind = ErrorKind.Validation)
        {
            Succeeded = succeeded;
            Value = value;
            Errors = errors.ToList();
            Kind = kind;
        }

        public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());

        public static Result<T> Failure(string error) => new(false, default, new[] { error });

        public static Result<T> Failure(string error, ErrorKind kind) => new(false, default, new[] { error }, kind);

        public static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
    }
}
