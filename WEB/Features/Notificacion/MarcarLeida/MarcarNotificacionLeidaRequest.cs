using WEB.Core.Mediator;
using WEB.Data;

namespace WEB.Features.Notificacion.MarcarLeida;

public record MarcarNotificacionLeidaRequest(int NotificacionId, string UsuarioId) 
    : IRequest<bool>, IRequireAuthorization{
    public string[] Roles => new[]{ AppRoles.JefeProceso,AppRoles.JefeArea };
}