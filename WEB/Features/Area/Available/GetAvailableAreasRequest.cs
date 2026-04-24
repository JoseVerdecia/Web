using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Users.Dto;

namespace WEB.Features.Area.Available;

public record GetAvailableAreasRequest() : IRequest<List<AvailableUserDto>>,IRequireAuthorization
{
    public string[] Roles => new []{AppRoles.Administrador};
}