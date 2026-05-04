using WEB.Core.Mediator;
using WEB.Data;

namespace WEB.Features.Users.Get;

public record GetPendingUsersCountRequest : IRequest<PendingCountDto>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}

public record PendingCountDto { public int Count { get; init; } }