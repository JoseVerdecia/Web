using System.Diagnostics;
using WEB.Core.Result;

namespace WEB.Core.Mediator;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Iniciando request {RequestName}", requestName);

            var result = await next();

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Request {RequestName} completado exitosamente en {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Request {RequestName} completado con errores en {ElapsedMilliseconds}ms - {@Errors}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    string.Join(" | ", result.Errors.Select(e => e.Message)));
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Request {RequestName} falló después de {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}