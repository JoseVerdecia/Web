using WEB.Models;

namespace WEB.Interfaces;

public interface INotificationService
{
    void ShowSuccess(string message);
    void ShowError(string message);
    
    Task<int> GetCountNoLeidasAsync(string usuarioId, CancellationToken cancellationToken);
    Task<IEnumerable<NotificacionModel>> GetNotificacionesAsync(string usuarioId, CancellationToken cancellationToken);
    Task<IEnumerable<NotificacionModel>> GetPendientesAsync(string usuarioId, CancellationToken cancellationToken);
    Task MarcarComoLeidaAsync(int notificacionId, string usuarioId, CancellationToken cancellationToken);
    Task MarcarTodasComoLeidasAsync(string usuarioId, CancellationToken cancellationToken);
    Task EnviarNotificacionAsync(NotificacionModel notificacion, CancellationToken cancellationToken);
}