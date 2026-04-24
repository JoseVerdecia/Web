using WEB.Common;
using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Proceso.Dto;

namespace WEB.Features.Proceso.GetAll;

public record GetAllProcesosRequest(int Page = 1, int PageSize = 10) : IRequest<PagedResult<ProcesoDto>>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador,AppRoles.JefeArea };
}
