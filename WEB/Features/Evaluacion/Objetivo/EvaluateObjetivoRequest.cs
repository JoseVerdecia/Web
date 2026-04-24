using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Evaluacion.Objetivo;

public record EvaluateObjetivoRequest(int id ):IRequest<ObjetivoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}