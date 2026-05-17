using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Denegar;

public class DenegarResponsableAreaHandler:IRequestHandler<DenegarResponsableAreaRequest,AreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IRoleManagementService _roleService;

    public DenegarResponsableAreaHandler(IUnitOfWorkAccessor uow, IRoleManagementService roleService)
    {
        _uow = uow;
        _roleService = roleService;
    }

    public async Task<AppResult<AreaDto>> Handle(DenegarResponsableAreaRequest request, CancellationToken cancellationToken)
    {
        AreaModel? area = await _uow.Current.Area.Get(a=>a.Id==request.AreaId,cancellationToken);
        if (area is null)
            return AppResult<AreaDto>.NotFound("Area no encontrado.");
        
        if(area.JefeAreaId != request.JefeAreaId)
            return AppResult<AreaDto>.Fail("El usuario no es el responsable de esta área.");
        
        var roleAppResult = await _roleService.ResetToDefaultRoleAsync(request.JefeAreaId);
        if (roleAppResult.IsFailure)
            return AppResult<AreaDto>.Fail(roleAppResult.Errors);
       
        area.JefeAreaId = null; 
        _uow.Current.Area.Update(area);
        await _uow.Current.SaveAsync();
        

        return AppResult<AreaDto>.Success(area.MapToDto());
    }
}