using WEB.Core.Mediator;
using WEB.Data;

namespace WEB.Features.Notificacion.Delete;

public record DeleteNotificacionRequest(int NotificacionId, string UsuarioId) 
    : IRequest<bool>, IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.JefeProceso, AppRoles.Administrador, AppRoles.JefeArea };
}