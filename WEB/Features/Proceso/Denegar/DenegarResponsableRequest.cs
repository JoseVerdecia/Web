using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Proceso.Denegar;

public record DenegarResponsableRequest(string JefeProcesoId,int? ProcesoId):IRequest<ProcesoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}