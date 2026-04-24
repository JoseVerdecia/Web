using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Proceso.Create;

public record CreateProcesoCommand(string Nombre) : IRequest<ProcesoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}