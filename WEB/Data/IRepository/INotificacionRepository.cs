using WEB.Models;

namespace WEB.Data.IRepository;


public interface INotificationRepository:IRepository<NotificacionModel>
{
    void Update(NotificacionModel solicitud);
    Task<int> CountNoLeidasAsync(string usuarioId, CancellationToken cancellationToken);
    
    Task<IEnumerable<NotificacionModel>> GetNoLeidasByUsuarioAsync(
        string usuarioId, 
        CancellationToken cancellationToken,
        string? includeProperties = null);
    
    Task<IEnumerable<NotificacionModel>> GetByUsuarioAsync(
        string usuarioId, 
        CancellationToken cancellationToken,
        string? includeProperties = null);
    
    Task<IEnumerable<NotificacionModel>> GetPendientesByUsuarioAsync(
        string usuarioId, 
        CancellationToken cancellationToken,
        string? includeProperties = null);
    
    Task MarcarComoLeidaAsync(int notificacionId, CancellationToken cancellationToken);
    Task MarcarTodasComoLeidasAsync(string usuarioId, CancellationToken cancellationToken);

    Task<List<NotificacionModel>> GetByIndicadorIdAsync(int indicadorId, CancellationToken cancellationToken);
}