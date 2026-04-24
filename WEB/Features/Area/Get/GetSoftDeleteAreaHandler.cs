using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Area.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Get;

public class GetSoftDeleteAreaHandler:IRequestHandler<GetSoftDeleteAreaRequest,AreaDto>
{
    private readonly IUnitOfWorkAccessor _unitOfWork;

    public GetSoftDeleteAreaHandler(IUnitOfWorkAccessor unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<AreaDto>> Handle(GetSoftDeleteAreaRequest request, CancellationToken cancellationToken)
    {
        AreaModel? areaSoftDeleted = await _unitOfWork.Current.Area.GetIncludingDeleted(a => a.IsDeleted == true && a.Id == request.Id,cancellationToken);
        
        return areaSoftDeleted == null 
            ? Result<AreaDto>.NotFound("Área eliminada no encontrada") 
            : Result<AreaDto>.Success(areaSoftDeleted.MapToDto());
    }
}