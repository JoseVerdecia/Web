using WEB.Core.Helpers;
using WEB.Core.Result;
using WEB.Core.Interfaces;

namespace WEB.Core.Mediator;

public class ErrorToastBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly INotificationService _notificationService;

    public ErrorToastBehavior(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<AppResult<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await next();

        if (result.IsFailure)
        {
            ErrorNotification.ErrorToast(result, _notificationService);
        }

        return result;
    }
}