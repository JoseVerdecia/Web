using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Proceso.Get;

public record GetProcesoByJefeIdRequest(string JefeProcesoId) 
    : IRequest<ProcesoDto?>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.JefeProceso };
}