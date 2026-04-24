using Microsoft.EntityFrameworkCore;
using WEB.Core.Result;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Data.Repository;
using WEB.Interfaces;

namespace WEB.Core.Mediator;

public class UnitOfWorkScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public UnitOfWorkScopeBehavior(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Crear un nuevo DbContext para este request
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Crear UnitOfWork con ese DbContext
        var unitOfWork = new UnitOfWork(dbContext);
        var scope = new UnitOfWorkScope(unitOfWork);

        // Establecer el ámbito actual (accesible por el accesor)
        UnitOfWorkAccessor.CurrentScope = scope;

        try
        {
            return await next();
        }
        finally
        {
            UnitOfWorkAccessor.CurrentScope = null;
        }
    }

    private class UnitOfWorkScope : IUnitOfWorkScope
    {
        public IUnitOfWork UnitOfWork { get; }
        public UnitOfWorkScope(IUnitOfWork uow) => UnitOfWork = uow;
    }
}