using Microsoft.EntityFrameworkCore.Storage;

namespace WEB.Data.IRepository;

public interface IUnitOfWork : IDisposable
{
    IProcesoRepository Proceso { get; }
    IAreaRepository Area { get; }
    IObjetivoRepository Objetivo { get; }
    IIndicadorRepository Indicador { get; }
    IIndicadorDeAreaRepository IndicadorDeArea { get; }
    INotificationRepository Notificacion { get; }
    
    Task SaveAsync();
    void Save();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}