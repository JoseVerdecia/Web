using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Delete;

public class DeleteAreaHandler : IRequestHandler<DeleteAreaRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteCascadeService;
    private readonly IRoleManagementService _roleService;

    public DeleteAreaHandler(
        IUnitOfWorkAccessor uow,
        IDeleteCascadeService cascadeService,
        IRoleManagementService roleService)
    {
        _uow = uow;
        _deleteCascadeService = cascadeService;
        _roleService = roleService;
    }

    public async Task<AppResult<Unit>> Handle(DeleteAreaRequest request, CancellationToken cancellationToken)
    {
        AreaModel? area = await _uow.Current.Area.GetIncludingDeleted(a => a.Id == request.Id,cancellationToken);

        if (area == null) 
            return AppResult<Unit>.NotFound("Área no encontrada");

        if (area.IsDeleted && !request.Permanent)
            return AppResult<Unit>.Fail("El área ya está eliminada.");
        
        if (!string.IsNullOrEmpty(area.JefeAreaId))
        {
            var resetAppResult = await _roleService.ResetToDefaultRoleAsync(area.JefeAreaId);
            area.JefeAreaId = null;
            area.JefeArea=null;
            
            if (resetAppResult.IsFailure)
                return AppResult<Unit>.Fail($"No se pudo resetear el rol del jefe de área: {resetAppResult.Errors}");
        }
        
        if (request.Permanent)
        {
            // HARD DELETE
            _uow.Current.Area.Delete(area);
        }
        else
        {
            // SOFT DELETE
            if (area.IsDeleted) 
                return AppResult<Unit>.Fail("El área ya fue eliminada anteriormente.");

            await _deleteCascadeService.SoftDeleteArea(area);
        }

        await _uow.Current.SaveAsync();
        
        return AppResult<Unit>.Success(Unit.Value);
    }
}