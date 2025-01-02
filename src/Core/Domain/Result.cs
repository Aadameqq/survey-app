namespace Core.Domain;

public readonly struct Result<T>
{
    private enum ResultState
    {
        Null,
        Failure,
        Success
    }

    private readonly ResultState _state;

    public T Value { get; }
    public Exception Exception { get; }

    public bool IsSuccess => _state == ResultState.Success;
    public bool IsFailure => _state == ResultState.Failure;
    public bool IsNull => _state == ResultState.Null;

    public Result(T value)
    {
        Value = value;
        Exception = null!;
        _state = ResultState.Success;
    }

    public Result(Exception exception)
    {
        Value = default!;
        Exception = exception;
        _state = ResultState.Failure;
    }

    public static implicit operator Result<T>(T? value)
    {
        return value is not null ? new Result<T>(value) : new Result<T>();
    }

    public static implicit operator Result<T>(Exception exception)
    {
        return new Result<T>(exception);
    }

    public static implicit operator Result(Result<T> result)
    {
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Exception);
    }
}

public readonly struct Result(Exception exception)
{
    public Exception Exception { get; } = exception;

    public bool IsSuccess => Exception is null;
    public bool IsFailure => Exception is not null;

    public static implicit operator Result(Exception exception)
    {
        return new Result(exception);
    }

    public static Result Success()
    {
        return new Result();
    }

    public static Result Failure(Exception exception)
    {
        return new Result(exception);
    }
}
