using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Objetivo.Delete;

public record DeleteObjetivosRequest(IEnumerable<int> Ids, bool Permanent = false) : IRequest<Unit>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}