namespace WEB.Core.Result;

public static class ResultExtensions
{
    public static AppResult<TOut> Bind<TIn, TOut>(
        this AppResult<TIn> appResult,
        Func<TIn, AppResult<TOut>> func)
    {
        if (appResult.IsFailure)
            return AppResult<TOut>.Fail(appResult.Errors);
        return func(appResult.Value!);
    }

    public static async Task<AppResult<TOut>> BindAsync<TIn, TOut>(
        this AppResult<TIn> appResult,
        Func<TIn, Task<AppResult<TOut>>> func)
    {
        if (appResult.IsFailure)
            return AppResult<TOut>.Fail(appResult.Errors);
        return await func(appResult.Value!);
    }

    public static AppResult<TOut> Map<TIn, TOut>(
        this AppResult<TIn> appResult,
        Func<TIn, TOut> mapper)
    {
        if (appResult.IsFailure)
            return AppResult<TOut>.Fail(appResult.Errors);
        return AppResult<TOut>.Success(mapper(appResult.Value!));
    }

    public static AppResult<T> Tap<T>(
        this AppResult<T> appResult,
        Action<T> action)
    {
        if (appResult.IsSuccess)
            action(appResult.Value);
        return appResult;
    }

    public static async Task<AppResult<T>> TapAsync<T>(
        this AppResult<T> appResult,
        Func<T, Task> action)
    {
        if (appResult.IsSuccess)
            await action(appResult.Value!);
        return appResult;
    }

   

    public static async Task<AppResult<TOut>> BindAsync<TIn, TOut>(
        this Task<AppResult<TIn>> resultTask,
        Func<TIn, Task<AppResult<TOut>>> func)
    {
        var result = await resultTask;
        return await result.BindAsync(func);
    }

    public static async Task<AppResult<TOut>> Map<TIn, TOut>(
        this Task<AppResult<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<AppResult<T>> Tap<T>(
        this Task<AppResult<T>> resultTask,
        Action<T> action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    public static async Task<AppResult<T>> TapAsync<T>(
        this Task<AppResult<T>> resultTask,
        Func<T, Task> action)
    {
        var result = await resultTask;
        return await result.TapAsync(action);
    }
    
    
}