using WEB.Common;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.GetAll;

public record GetSoftDeletedIndicadoresByJefeProcesoRequest(
    int Page,
    int PageSize,
    string JefeProcesoId
) : IRequest<PagedResult<IndicadorDto>>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.JefeProceso };
}