using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Area.Dto;

namespace WEB.Features.Area.Denegar;

public record DenegarResponsableAreaRequest(string JefeAreaId,int? AreaId):IRequest<AreaDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}