using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;

namespace WEB.Features.Proceso.Delete;

public class DeleteProcesosHandler : IRequestHandler<DeleteProcesosRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteCascadeService;
    
    public DeleteProcesosHandler(IUnitOfWorkAccessor uow,IDeleteCascadeService deleteCascadeService)
    {
        _uow = uow;
        _deleteCascadeService = deleteCascadeService;
    }

    public async Task<AppResult<Unit>> Handle(DeleteProcesosRequest request, CancellationToken cancellationToken)
    {
        var ids = request.Ids.ToList();
        if (!ids.Any()) return AppResult<Unit>.Fail("No se proporcionaron procesos.");

        var procesos = await _uow.Current.Proceso.GetAllByIncludingDeleted(p => ids.Contains(p.Id), cancellationToken);
        if (!procesos.Any()) return AppResult<Unit>.NotFound("No se encontraron los procesos.");

        if (request.Permanent)
        {
            _uow.Current.Proceso.DeleteRange(procesos);
            await _uow.Current.SaveAsync();
        }
        else
        {
            foreach (var proceso in procesos.Where(p => !p.IsDeleted).ToList())
            {
                await _deleteCascadeService.SoftDeleteProceso(proceso);
            }
        }

        await _uow.Current.SaveAsync();
        return AppResult<Unit>.Success(Unit.Value);
    }
}