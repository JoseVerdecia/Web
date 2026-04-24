using Microsoft.AspNetCore.OutputCaching;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Delete;

public class DeleteProcesoHandler : IRequestHandler<DeleteProcesoRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteCascadeService;
    private readonly IRoleManagementService _roleService;

    public DeleteProcesoHandler(
        IUnitOfWorkAccessor uow,
        IDeleteCascadeService cascadeService,
        IRoleManagementService roleService)
    {
        _uow = uow;
        _deleteCascadeService = cascadeService;
        _roleService = roleService;
    }

    public async Task<Result<Unit>> Handle(DeleteProcesoRequest request, CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.GetIncludingDeleted(a => a.Id == request.Id,cancellationToken,includeProperties:"Indicadores");

        if (proceso == null) 
            return Result<Unit>.NotFound("Área no encontrada");

        if (proceso.IsDeleted && !request.Permanent)
            return Result<Unit>.Fail("El proceso ya está eliminado.");
        
        if (!string.IsNullOrEmpty(proceso.JefeProcesoId))
        {
            var resetResult = await _roleService.ResetToDefaultRoleAsync(proceso.JefeProcesoId);
            proceso.JefeProcesoId = null;
            proceso.JefeProceso = null;
            if (resetResult.IsFailure)
                return Result<Unit>.Fail($"No se pudo resetear el rol del jefe de proceso: {resetResult.Errors}");
        }
        
        if (request.Permanent)
        {
            // HARD DELETE
            _uow.Current.Proceso.Delete(proceso);
        }
        else
        {
            // SOFT DELETE
            if (proceso.IsDeleted) 
                return Result<Unit>.Fail("El área ya fue eliminada anteriormente.");

            await _deleteCascadeService.SoftDeleteProceso(proceso);
        }

        await _uow.Current.SaveAsync();
       // await _cacheStore.InvalidateSoftDeleteCache(CacheTags.ProcesoSoftDelete,CacheTags.AllProcesosSoftDelete);
        return Result<Unit>.Success(Unit.Value);
    }
}