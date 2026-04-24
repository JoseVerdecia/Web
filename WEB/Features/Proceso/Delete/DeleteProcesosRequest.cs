using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Proceso.Delete;

public record DeleteProcesosRequest(IEnumerable<int> Ids, bool Permanent = false) : IRequest<Unit>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}