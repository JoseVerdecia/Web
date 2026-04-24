using WEB.Common;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.Get;

public record GetIndicadoresByProcesoRequest(int ProcesoId, int Page = 1, int PageSize = 10) 
    : IRequest<PagedResult<IndicadorDto>>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador, AppRoles.JefeProceso };
}