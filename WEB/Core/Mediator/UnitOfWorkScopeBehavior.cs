using Microsoft.EntityFrameworkCore;
using WEB.Core.Result;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Data.Repository;

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
        
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

      
        var unitOfWork = new UnitOfWork(dbContext);
        var scope = new UnitOfWorkScope(unitOfWork);

      
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