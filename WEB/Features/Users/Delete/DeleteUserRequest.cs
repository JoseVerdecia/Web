using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Users.Delete;

public record DeleteUserRequest(string UserId) : IRequest<Unit>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}