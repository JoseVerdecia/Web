using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Notificacion.Dto;

namespace WEB.Features.Notificacion.GetCount;

public record GetNotificacionCountRequest(string UsuarioId) : IRequest<NotificacionCountDto>, IRequireAuthorization
{
    public string[] Roles => new[]{ AppRoles.JefeProceso,AppRoles.JefeArea };
}