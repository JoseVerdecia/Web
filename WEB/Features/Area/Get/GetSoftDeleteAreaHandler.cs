using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Get;

public class GetSoftDeleteAreaHandler:IRequestHandler<GetSoftDeleteAreaRequest,AreaDto>
{
    private readonly IUnitOfWorkAccessor _unitOfWork;

    public GetSoftDeleteAreaHandler(IUnitOfWorkAccessor unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<AppResult<AreaDto>> Handle(GetSoftDeleteAreaRequest request, CancellationToken cancellationToken)
    {
        AreaModel? areaSoftDeleted = await _unitOfWork.Current.Area.GetIncludingDeleted(a => a.IsDeleted == true && a.Id == request.Id,cancellationToken);
        
        return areaSoftDeleted == null 
            ? AppResult<AreaDto>.NotFound("Área eliminada no encontrada") 
            : AppResult<AreaDto>.Success(areaSoftDeleted.MapToDto());
    }
}