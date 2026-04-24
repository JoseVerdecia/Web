using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Area.Dto;

namespace WEB.Features.Area.Assign;

public record AsignarResponsableAreaRequest(string UsuarioId, int AreaId):IRequest<AreaDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}