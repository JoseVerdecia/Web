using WEB.Common;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Objetivo.GetAll;

public record GetAllObjetivosRequest(int Page = 1, int PageSize = 10) : IRequest<PagedResult<ObjetivoDto>>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador,AppRoles.JefeProceso };
}
