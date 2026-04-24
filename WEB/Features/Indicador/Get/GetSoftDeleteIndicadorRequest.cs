using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.Get;

public record GetSoftDeleteIndicadorRequest(int Id):IRequest<IndicadorDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador, AppRoles.JefeProceso };
}