using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Assign;

public class AsignarResponsableAreaHandler:IRequestHandler<AsignarResponsableAreaRequest,AreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IUserService _userService;
    private readonly IRoleManagementService _roleService;
    
    public AsignarResponsableAreaHandler(IUnitOfWorkAccessor uow, IUserService userService, IRoleManagementService roleService)
    {
        _uow = uow;
        _userService = userService;
        _roleService = roleService;
    }

    public async Task<AppResult<AreaDto>> Handle(AsignarResponsableAreaRequest areaRequest, CancellationToken cancellationToken)
    {
        AreaModel? area = await _uow.Current.Area.Get(p => p.Id == areaRequest.AreaId,cancellationToken);
        
        if (area is null)
            return AppResult<AreaDto>.NotFound("Este area no existe");
        
        if(area.JefeAreaId is not null)
            return AppResult<AreaDto>.Fail("Esta área ya tiene un responsable");
        
        var userAppResult = await _userService.EnsureUserIsAvailableForResponsibilityAsync(areaRequest.UsuarioId,cancellationToken);
        if (userAppResult.IsFailure) return AppResult<AreaDto>.Fail(userAppResult.Errors);
        

        var roleAppResult = await _roleService.UpgradeToJefeAreaAsync(areaRequest.UsuarioId);
        
        if (roleAppResult.IsFailure)
            return AppResult<AreaDto>.Fail(roleAppResult.Errors);
        
        area.JefeAreaId = areaRequest.UsuarioId;
        _uow.Current.Area.Update(area);
        await _uow.Current.SaveAsync();
        
        return AppResult<AreaDto>.Success(area.MapToDto());
    }
}