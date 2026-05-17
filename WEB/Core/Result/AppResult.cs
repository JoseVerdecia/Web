using WEB.Common;
using WEB.Enums;

namespace WEB.Core.Result;

public class AppResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public List<ErrorDetail> Errors { get; }

    protected AppResult(bool isSuccess, List<ErrorDetail> errors)
    {
        if (isSuccess && errors.Any()) throw new InvalidOperationException();
        if (!isSuccess && !errors.Any()) throw new InvalidOperationException();
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static AppResult Success() => new(true, new());
    public static AppResult Fail(string message) => new(false, new() { new ErrorDetail { Message = message } });
    public static AppResult Fail(List<ErrorDetail> errors) => new(false, errors);
    public static AppResult Unauthorized(string message = "No autorizado") => new(false, new() { new ErrorDetail { Message = message, Type = ErrorType.Unauthorized } });
    public static AppResult Forbidden(string message = "Acceso denegado") => new(false, new() { new ErrorDetail { Message = message, Type = ErrorType.Forbidden } });
    public static AppResult NotFound(string message = "Recurso no encontrado") => new(false, new() { new ErrorDetail { Message = message, Type = ErrorType.NotFound } });
}

public class AppResult<T> : AppResult
{
    public T? Value { get; }
    private AppResult(bool isSuccess, T? value, List<ErrorDetail> errors) : base(isSuccess, errors) => Value = value;

    public static AppResult<T> Success(T value) => new(true, value, new());
    public new static AppResult<T> Fail(string message) => new(false, default, new() { new ErrorDetail { Message = message } });
    public new static AppResult<T> Fail(List<ErrorDetail> errors) => new(false, default, errors);
    public new static AppResult<T> Unauthorized(string message = "No autorizado") => new(false, default, new() { new ErrorDetail { Message = message, Type = ErrorType.Unauthorized } });
    public new static AppResult<T> Forbidden(string message = "Acceso denegado") => new(false, default, new() { new ErrorDetail { Message = message, Type = ErrorType.Forbidden } });
    public new static AppResult<T> NotFound(string message = "Recurso no encontrado") => new(false, default, new() { new ErrorDetail { Message = message, Type = ErrorType.NotFound } });
}