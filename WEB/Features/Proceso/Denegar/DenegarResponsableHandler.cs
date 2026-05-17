using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Denegar;

public class DenegarResponsableHandler:IRequestHandler<DenegarResponsableRequest,ProcesoDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IRoleManagementService _roleService;

    public DenegarResponsableHandler(IUnitOfWorkAccessor uow, IRoleManagementService roleService)
    {
        _uow = uow;
        _roleService = roleService;
    }

    public async Task<AppResult<ProcesoDto>> Handle(DenegarResponsableRequest request, CancellationToken cancellationToken)
    {
        
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(p=>p.Id ==request.ProcesoId,cancellationToken,includeProperties:"JefeProceso");
        
        if(proceso?.JefeProcesoId != request.JefeProcesoId)
            return AppResult<ProcesoDto>.Fail("El usuario no es el responsable de este proceso.");
        
        if (proceso is null)
            return AppResult<ProcesoDto>.NotFound("Proceso no encontrado.");
        
        var roleResult = await _roleService.ResetToDefaultRoleAsync(request.JefeProcesoId);
        if (roleResult.IsFailure)
            return AppResult<ProcesoDto>.Fail(roleResult.Errors);
       
        proceso.JefeProcesoId = null; 
        _uow.Current.Proceso.Update(proceso);
        await _uow.Current.SaveAsync();
        return AppResult<ProcesoDto>.Success(proceso.MapToDto());
    }
}