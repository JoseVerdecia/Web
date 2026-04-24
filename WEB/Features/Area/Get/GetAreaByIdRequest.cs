using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Area.Dto;

namespace WEB.Features.Area.Get;

public record GetAreaByIdRequest(int Id) : IRequest<AreaDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador,AppRoles.JefeProceso,AppRoles.JefeArea };
}