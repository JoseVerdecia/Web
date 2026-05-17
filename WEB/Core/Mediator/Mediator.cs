using WEB.Core.Result;

namespace WEB.Core.Mediator;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<AppResult<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        
        var requestType = request.GetType();

      
        var behaviorInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));

        
        var behaviors = _serviceProvider
            .GetServices(behaviorInterfaceType)
            .Reverse()
            .ToList();

      
        RequestHandlerDelegate<TResponse> handlerDelegate = async () =>
        {
            var handlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(requestType, typeof(TResponse));

            var handler = _serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;

            return await (Task<AppResult<TResponse>>)method.Invoke(
                handler,
                new object[] { request, cancellationToken })!;
        };

        
        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
          
            handlerDelegate = () => ((dynamic)behavior).Handle((dynamic)request, next, cancellationToken);
        }

        return await handlerDelegate();
    }
}