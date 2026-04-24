using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Proceso.Delete;

public record DeleteProcesoRequest(int Id, bool Permanent = false) : IRequest<Unit>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}