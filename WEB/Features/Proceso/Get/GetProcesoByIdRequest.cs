using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Proceso.Get;

public record GetProcesoByIdRequest(int Id) : IRequest<ProcesoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador,AppRoles.JefeArea };
}