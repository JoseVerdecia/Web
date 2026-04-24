using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Notificacion.Dto;

namespace WEB.Features.Notificacion.GetByUser;

public record GetNotificacionesByUsuarioRequest(string UsuarioId)
    : IRequest<List<NotificacionDto>>, IRequireAuthorization
{
    public string[] Roles => new[]{ AppRoles.JefeProceso,AppRoles.JefeArea };
}