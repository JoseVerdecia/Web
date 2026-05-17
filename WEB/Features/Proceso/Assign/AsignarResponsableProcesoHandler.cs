using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Assign;

public class AsignarResponsableProcesoHandler:IRequestHandler<AsignarResponsableProcesoRequest,ProcesoDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IUserService _userService;
    private readonly IRoleManagementService _roleService;
    
    public AsignarResponsableProcesoHandler(IUnitOfWorkAccessor uow, IUserService userService, IRoleManagementService roleService)
    {
        _uow = uow;
        _userService = userService;
        _roleService = roleService;
    }

    public async Task<AppResult<ProcesoDto>> Handle(AsignarResponsableProcesoRequest procesoRequest, CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(p => p.Id == procesoRequest.ProcesoId,cancellationToken);
        
        if (proceso is null)
            return AppResult<ProcesoDto>.NotFound("Este proceso no existe");
        
        if (proceso.JefeProcesoId is not null)
            return AppResult<ProcesoDto>.Fail("Este proceso ya tiene un responsable");
        
        AppResult<ApplicationUser> userAppResult = await _userService.EnsureUserIsAvailableForResponsibilityAsync(procesoRequest.UsuarioId,cancellationToken);
        if (userAppResult.IsFailure) return AppResult<ProcesoDto>.Fail(userAppResult.Errors);

        AppResult roleAppResult = await _roleService.UpgradeToJefeProcesoAsync(procesoRequest.UsuarioId);
        if (roleAppResult.IsFailure)
            return AppResult<ProcesoDto>.Fail(roleAppResult.Errors);
        
        proceso.JefeProcesoId = procesoRequest.UsuarioId;
        _uow.Current.Proceso.Update(proceso);
        await _uow.Current.SaveAsync();
        return AppResult<ProcesoDto>.Success(proceso.MapToDto());
    }
}