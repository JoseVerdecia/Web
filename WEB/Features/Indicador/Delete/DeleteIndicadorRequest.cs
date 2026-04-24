using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Indicador.Delete;

public record DeleteIndicadorRequest(int Id, bool Permanent = false) : IRequest<Unit>,IRequireAuthorization
{
    public string[] Roles => new[]{AppRoles.Administrador, AppRoles.JefeProceso};
}