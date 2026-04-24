using Microsoft.AspNetCore.OutputCaching;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Area.Dto;
using WEB.Interfaces;
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

    public async Task<Result<AreaDto>> Handle(AsignarResponsableAreaRequest areaRequest, CancellationToken cancellationToken)
    {
        AreaModel? area = await _uow.Current.Area.Get(p => p.Id == areaRequest.AreaId,cancellationToken);
        
        if (area is null)
            return Result<AreaDto>.NotFound("Este area no existe");
        
        if(area.JefeAreaId is not null)
            return Result<AreaDto>.Fail("Esta área ya tiene un responsable");
        
        var userResult = await _userService.EnsureUserIsAvailableForResponsibilityAsync(areaRequest.UsuarioId,cancellationToken);
        if (userResult.IsFailure) return Result<AreaDto>.Fail(userResult.Errors);
        

        var roleResult = await _roleService.UpgradeToJefeAreaAsync(areaRequest.UsuarioId);
        
        if (roleResult.IsFailure)
            return Result<AreaDto>.Fail(roleResult.Errors);
        
        area.JefeAreaId = areaRequest.UsuarioId;
        _uow.Current.Area.Update(area);
        await _uow.Current.SaveAsync();
        
        return Result<AreaDto>.Success(area.MapToDto());
    }
}