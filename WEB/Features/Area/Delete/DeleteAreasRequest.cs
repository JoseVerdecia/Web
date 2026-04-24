using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Area.Delete;

public record DeleteAreasRequest(IEnumerable<int> Ids, bool Permanent = false) : IRequest<Unit>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}