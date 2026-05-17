using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Objetivo.Delete;

public class DeleteObjetivoHandler : IRequestHandler<DeleteObjetivoRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteCascadeService;
   // private readonly IOutputCacheStore _cacheStore;
    public DeleteObjetivoHandler(IUnitOfWorkAccessor uow, IDeleteCascadeService deleteCascadeService/*,IOutputCacheStore cacheStore*/)
    {
        _uow = uow;
        _deleteCascadeService = deleteCascadeService;
       // _cacheStore = cacheStore;
    }

    public async Task<AppResult<Unit>> Handle(DeleteObjetivoRequest request, CancellationToken cancellationToken)
    {
        ObjetivoModel? objetivo = await _uow.Current.Objetivo.GetIncludingDeleted(a => a.Id == request.Id,cancellationToken,includeProperties:"Indicadores");
        
        if (objetivo == null) 
            return AppResult<Unit>.NotFound("Objetivo no encontrado");
        
        
        if (request.Permanent)
        {
            await _deleteCascadeService.HardDeleteObjetivo(objetivo);
        }
        else
        {
            if (objetivo.IsDeleted) 
                return AppResult<Unit>.Fail("El objetivo ya fue eliminado anteriormente.");

            await _deleteCascadeService.SoftDeleteObjetivo(objetivo);
        }

        await _uow.Current.SaveAsync();
        //await _cacheStore.InvalidateSoftDeleteCache(CacheTags.ObjetivoSoftDelete,CacheTags.AllObjetivosSoftDelete);
        return AppResult<Unit>.Success(Unit.Value);
    }
}