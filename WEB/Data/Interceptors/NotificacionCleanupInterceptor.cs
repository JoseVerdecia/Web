using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WEB.Enums;
using WEB.Models;

namespace WEB.Data.Interceptors;

public class NotificacionCleanupInterceptor : SaveChangesInterceptor
{
    private const int DiasParaEliminar = 15;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        EliminarNotificacionesAntiguas(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        EliminarNotificacionesAntiguas(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EliminarNotificacionesAntiguas(DbContext? context)
    {
        if (context is null) return;

        var cutoffDate = DateTime.UtcNow.AddDays(-DiasParaEliminar);
        
        var notificacionesAntiguas = context.ChangeTracker
            .Entries<NotificacionModel>()
            .Where(e => e.Entity.Leida &&
                        e.Entity.Estado == EstadoNotificacion.Aceptada &&
                        e.Entity.LeidaAt.HasValue &&
                        e.Entity.LeidaAt.Value <= cutoffDate)
            .ToList();

        foreach (var entry in notificacionesAntiguas)
        {
            entry.State = EntityState.Deleted;
        }

        if (notificacionesAntiguas.Any())
        {
        }
    }
}