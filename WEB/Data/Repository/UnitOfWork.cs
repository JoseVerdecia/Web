using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WEB.Data.IRepository;

namespace WEB.Data.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Constructor usado por el Behavior (recibe DbContext explícito)
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Area = new AreaRepository(_context);
        Notificacion = new NotificacionRepository(_context);
        Objetivo = new ObjetivoRepository(_context);
        Proceso = new ProcesoRepository(_context);
        Indicador = new IndicadorRepository(_context);
        IndicadorDeArea = new IndicadorDeAreaRepository(_context);
    }

    public IAreaRepository Area { get; }
    public INotificationRepository Notificacion { get; }
    public IObjetivoRepository Objetivo { get; }
    public IProcesoRepository Proceso { get; }
    public IIndicadorRepository Indicador { get; }
    public IIndicadorDeAreaRepository IndicadorDeArea { get; }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
    public void Save() => _context.SaveChanges();

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public async Task CommitAsync() => await SaveAsync();
    public async Task RollbackAsync() { }

    public void Dispose() => _context.Dispose();
}