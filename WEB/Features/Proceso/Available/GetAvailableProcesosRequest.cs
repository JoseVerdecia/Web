using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Users.Dto;

namespace WEB.Features.Proceso.Available;

public record GetAvailableProcesosRequest() : IRequest<List<AvailableUserDto>>,IRequireAuthorization
{
    public string[] Roles => new []{AppRoles.Administrador};
}