using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Proceso.Update;

public record UpdateProcesoCommand(
    int Id,
    string Nombre
) : IRequest<ProcesoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}

