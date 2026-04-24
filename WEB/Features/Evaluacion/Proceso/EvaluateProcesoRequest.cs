using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Evaluacion.Proceso;

public record EvaluateProcesoRequest(int id ):IRequest<ProcesoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}