namespace CleanArchitecture.Application.Abstractions.ROP
{
    // Railway/Result with multi-error support
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IReadOnlyList<Error> Errors { get; }

        protected Result(bool isSuccess, IReadOnlyList<Error> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Ok() => new(true, Array.Empty<Error>());
        public static Result Fail(params Error [] errors) => new(false, errors);

        // Lift to generic
        public static Result<T> Fail<T>(params Error [] errors) => Result<T>.Fail(errors);
    }

    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value) : base(true, Array.Empty<Error>()) => Value = value;
        private Result(IReadOnlyList<Error> errors) : base(false, errors) => Value = default;

        public static Result<T> Ok(T value) => new(value);
        public static new Result<T> Fail(params Error [] errors) => new(errors);

        // Fun helpers (ROP-ish)
        public Result<TOut> Map<TOut>(Func<T, TOut> map)
            => IsSuccess ? Result<TOut>.Ok(map(Value!)) : Result<TOut>.Fail(Errors.ToArray());

        public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> next)
            => IsSuccess ? await next(Value!) : Result<TOut>.Fail(Errors.ToArray());

        public Result<T> Ensure(Func<T, bool> predicate, Error error)
            => IsSuccess && !predicate(Value!) ? Fail<T>(error) : this;
    }
}
