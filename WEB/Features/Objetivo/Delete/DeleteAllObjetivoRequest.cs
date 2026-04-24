using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;

namespace WEB.Features.Objetivo.Delete;

public record DeleteAllObjetivoRequest(bool Permanent = false):IRequest<Unit>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}