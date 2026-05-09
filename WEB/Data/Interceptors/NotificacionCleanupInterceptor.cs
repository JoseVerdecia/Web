using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WEB.Enums;
using WEB.Models;

namespace WEB.Data.Interceptors;

public class NotificacionCleanupInterceptor : SaveChangesInterceptor
{
    private const int DiasParaEliminarLeidas = 365;  
    private const int DiasParaEliminarSoftDeleted = 365; 

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
        await EliminarNotificacionesAntiguasAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EliminarNotificacionesAntiguas(DbContext? context)
    {
        if (context is null) return;

       
        var cutoffLeidas = DateTime.UtcNow.AddDays(-DiasParaEliminarLeidas);
        var leidasParaBorrar = context.ChangeTracker
            .Entries<NotificacionModel>()
            .Where(e => e.Entity.Leida &&
                        e.Entity.Estado == EstadoNotificacion.Aceptada &&
                        e.Entity.LeidaAt.HasValue &&
                        e.Entity.LeidaAt.Value <= cutoffLeidas)
            .ToList();

        foreach (var entry in leidasParaBorrar)
            entry.State = EntityState.Deleted;

      
        var cutoffSoft = DateTime.UtcNow.AddDays(-DiasParaEliminarSoftDeleted);
        var softDeletedParaBorrar = context.Set<NotificacionModel>()
            .IgnoreQueryFilters()
            .Where(n => n.IsDeleted && n.DeletedAt <= cutoffSoft)
            .ToList();

        if (softDeletedParaBorrar.Any())
            context.Set<NotificacionModel>().RemoveRange(softDeletedParaBorrar);
    }

    private async Task EliminarNotificacionesAntiguasAsync(DbContext? context, CancellationToken ct)
    {
        if (context is null) return;

        var cutoffLeidas = DateTime.UtcNow.AddDays(-DiasParaEliminarLeidas);
        var leidasParaBorrar = context.ChangeTracker
            .Entries<NotificacionModel>()
            .Where(e => e.Entity.Leida &&
                        e.Entity.Estado == EstadoNotificacion.Aceptada &&
                        e.Entity.LeidaAt.HasValue &&
                        e.Entity.LeidaAt.Value <= cutoffLeidas)
            .ToList();

        foreach (var entry in leidasParaBorrar)
            entry.State = EntityState.Deleted;

        var cutoffSoft = DateTime.UtcNow.AddDays(-DiasParaEliminarSoftDeleted);
        var softDeletedParaBorrar = await context.Set<NotificacionModel>()
            .IgnoreQueryFilters()
            .Where(n => n.IsDeleted && n.DeletedAt <= cutoffSoft)
            .ToListAsync(ct);

        if (softDeletedParaBorrar.Any())
            context.Set<NotificacionModel>().RemoveRange(softDeletedParaBorrar);
    }
}