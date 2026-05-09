using Microsoft.EntityFrameworkCore;
using WEB.Data.IRepository;
using WEB.Models;

namespace WEB.Data.Repository;

public class NotificacionRepository : Repository<NotificacionModel>, INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificacionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    public void Update(NotificacionModel solicitud)
    {
        _context.Update(solicitud);
    }
    
   public async Task<int> CountNoLeidasAsync(string usuarioId, CancellationToken cancellationToken)
    {
        return await dbSet
            .Where(n => n.DestinatarioId == usuarioId && !n.Leida && !n.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificacionModel>> GetNoLeidasByUsuarioAsync(
        string usuarioId, 
        CancellationToken cancellationToken,
        string? includeProperties = null)
    {
        IQueryable<NotificacionModel> query = dbSet
            .Where(n => n.DestinatarioId == usuarioId && !n.Leida && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt);

        query = IncludeProperties(query, includeProperties);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificacionModel>> GetByUsuarioAsync(
        string usuarioId, 
        CancellationToken cancellationToken,
        string? includeProperties = null)
    {
        IQueryable<NotificacionModel> query = dbSet
            .Where(n => n.DestinatarioId == usuarioId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt);

        query = IncludeProperties(query, includeProperties);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificacionModel>> GetPendientesByUsuarioAsync(
        string usuarioId, 
        CancellationToken cancellationToken,
        string? includeProperties = null)
    {
        IQueryable<NotificacionModel> query = dbSet
            .Where(n => n.DestinatarioId == usuarioId 
                     && n.Estado == Enums.EstadoNotificacion.Pendiente 
                     && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt);

        query = IncludeProperties(query, includeProperties);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task MarcarComoLeidaAsync(int notificacionId, CancellationToken cancellationToken)
    {
        var notificacion = await dbSet.FindAsync(new object[] { notificacionId }, cancellationToken);
        
        if (notificacion != null && !notificacion.Leida)
        {
            notificacion.Leida = true;
            notificacion.LeidaAt = DateTime.UtcNow;
            _context.Update(notificacion);
        }
    }

    public async Task MarcarTodasComoLeidasAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var noLeidas = await dbSet
            .Where(n => n.DestinatarioId == usuarioId && !n.Leida && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var notificacion in noLeidas)
        {
            notificacion.LeidaAt = DateTime.UtcNow;
        }

        _context.UpdateRange(noLeidas);
    }
    
    public async Task<List<NotificacionModel>> GetByIndicadorIdAsync(int indicadorId, CancellationToken cancellationToken)
    {
        return await _context.Set<NotificacionModel>()
            .IgnoreQueryFilters()
            .Include(n => n.Remitente)
            .Include(n => n.Destinatario)
            .Include(n => n.IndicadorDeArea).ThenInclude(ia => ia.Area)
            .Include(n => n.IndicadorDeArea).ThenInclude(ia => ia.Indicador)
            .Where(n => n.IndicadorDeArea != null && n.IndicadorDeArea.IndicadorId == indicadorId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}