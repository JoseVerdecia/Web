using WEB.Core.Mediator;
using WEB.Data;
using WEB.Enums;
using WEB.Features.Area.Dto;

namespace WEB.Features.Area.Create;

public record CreateAreaCommand(string Nombre, AreaTipo Tipo) : IRequest<AreaDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}