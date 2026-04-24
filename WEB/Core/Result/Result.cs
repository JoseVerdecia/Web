using WEB.Common;
using WEB.Enums;

namespace WEB.Core.Result;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public List<ErrorDetail> Errors { get; }

    protected Result(bool isSuccess, List<ErrorDetail> errors)
    {
        if (isSuccess && errors.Any()) throw new InvalidOperationException();
        if (!isSuccess && !errors.Any()) throw new InvalidOperationException();
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, new());
    public static Result Fail(string message) => new(false, new() { new ErrorDetail { Message = message } });
    public static Result Fail(List<ErrorDetail> errors) => new(false, errors);
    public static Result Unauthorized(string message = "No autorizado") => new(false, new() { new ErrorDetail { Message = message, Type = ErrorType.Unauthorized } });
    public static Result Forbidden(string message = "Acceso denegado") => new(false, new() { new ErrorDetail { Message = message, Type = ErrorType.Forbidden } });
    public static Result NotFound(string message = "Recurso no encontrado") => new(false, new() { new ErrorDetail { Message = message, Type = ErrorType.NotFound } });
}

public class Result<T> : Result
{
    public T? Value { get; }
    private Result(bool isSuccess, T? value, List<ErrorDetail> errors) : base(isSuccess, errors) => Value = value;

    public static Result<T> Success(T value) => new(true, value, new());
    public new static Result<T> Fail(string message) => new(false, default, new() { new ErrorDetail { Message = message } });
    public new static Result<T> Fail(List<ErrorDetail> errors) => new(false, default, errors);
    public new static Result<T> Unauthorized(string message = "No autorizado") => new(false, default, new() { new ErrorDetail { Message = message, Type = ErrorType.Unauthorized } });
    public new static Result<T> Forbidden(string message = "Acceso denegado") => new(false, default, new() { new ErrorDetail { Message = message, Type = ErrorType.Forbidden } });
    public new static Result<T> NotFound(string message = "Recurso no encontrado") => new(false, default, new() { new ErrorDetail { Message = message, Type = ErrorType.NotFound } });
}