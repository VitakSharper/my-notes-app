namespace QuestionService.Data.Functional;

/// <summary>
/// A discriminated union representing either a successful result or an error.
/// Inspired by functional programming Railway Oriented Programming pattern.
/// </summary>
public abstract record Result<TValue>
{
    public sealed record Success(TValue Value) : Result<TValue>;
    public sealed record Failure(Error Error) : Result<TValue>;

    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Error),
            _ => throw new InvalidOperationException("Invalid result state")
        };

    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure) =>
        this switch
        {
            Success s => await onSuccess(s.Value),
            Failure f => await onFailure(f.Error),
            _ => throw new InvalidOperationException("Invalid result state")
        };

    public Result<TNew> Map<TNew>(Func<TValue, TNew> mapper) =>
        this switch
        {
            Success s => new Result<TNew>.Success(mapper(s.Value)),
            Failure f => new Result<TNew>.Failure(f.Error),
            _ => throw new InvalidOperationException("Invalid result state")
        };

    public async Task<Result<TNew>> MapAsync<TNew>(Func<TValue, Task<TNew>> mapper) =>
        this switch
        {
            Success s => new Result<TNew>.Success(await mapper(s.Value)),
            Failure f => new Result<TNew>.Failure(f.Error),
            _ => throw new InvalidOperationException("Invalid result state")
        };

    public Result<TNew> Bind<TNew>(Func<TValue, Result<TNew>> binder) =>
        this switch
        {
            Success s => binder(s.Value),
            Failure f => new Result<TNew>.Failure(f.Error),
            _ => throw new InvalidOperationException("Invalid result state")
        };

    public static implicit operator Result<TValue>(TValue value) => new Success(value);
    public static implicit operator Result<TValue>(Error error) => new Failure(error);
}

public sealed record Error(string Code, string Message)
{
    public static Error NotFound(string entity, string id) => 
        new("NotFound", $"{entity} with id '{id}' was not found");
    
    public static Error Unauthorized(string message = "User is not authorized") => 
        new("Unauthorized", message);
    
    public static Error Validation(string message) => 
        new("Validation", message);
    
    public static Error Conflict(string message) => 
        new("Conflict", message);
}

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T? value, Error errorIfNull) where T : class =>
        value is not null 
            ? new Result<T>.Success(value) 
            : new Result<T>.Failure(errorIfNull);

    public static Result<T> ToResult<T>(this T? value, Error errorIfNull) where T : struct =>
        value.HasValue 
            ? new Result<T>.Success(value.Value) 
            : new Result<T>.Failure(errorIfNull);

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> task, Error errorIfNull) where T : class =>
        (await task).ToResult(errorIfNull);

    public static async Task<TResult> MapAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> mapper)
    {
        var result = await resultTask;
        return result.Match(mapper, _ => default!);
    }

    public static async Task<Result<TNew>> ThenMap<TValue, TNew>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TNew> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }
}
