using WEB.Core.Result;
using WEB.Core.Interfaces;

namespace WEB.Core.Mediator;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUser _currentUser;
    public AuthorizationBehavior(ICurrentUser currentUser) => _currentUser = currentUser;

    public async Task<AppResult<TResponse>> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not IRequireAuthorization authRequest)
            return await next();

        if (!_currentUser.IsAuthenticated)
            return AppResult<TResponse>.Unauthorized();

        if (authRequest.Roles.Any() && !authRequest.Roles.Any(role => _currentUser.IsInRole(role)))
            return AppResult<TResponse>.Forbidden();

        return await next();
    }
}