using WEB.Core.Result;

namespace WEB.Core.Mediator;

public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<AppResult<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IMediator
{
    Task<AppResult<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

public interface IRequireAuthorization
{
    string[] Roles { get; }
}