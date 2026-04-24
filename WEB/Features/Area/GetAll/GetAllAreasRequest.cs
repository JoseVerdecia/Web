using WEB.Common;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Area.Dto;

namespace WEB.Features.Area.GetAll;

public record GetAllAreasRequest(int Page = 1, int PageSize = 10) : IRequest<PagedResult<AreaDto>>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador,AppRoles.JefeProceso };
}