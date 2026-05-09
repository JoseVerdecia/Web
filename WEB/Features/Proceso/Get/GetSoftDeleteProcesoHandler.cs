using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Get;

public class GetSoftDeleteProcesoHandler:IRequestHandler<GetSoftDeleteProcesoRequest,ProcesoDto>
{
    private readonly IUnitOfWorkAccessor _unitOfWork;

    public GetSoftDeleteProcesoHandler(IUnitOfWorkAccessor unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<ProcesoDto>> Handle(GetSoftDeleteProcesoRequest request, CancellationToken cancellationToken)
    {
        ProcesoModel? procesoSoftDeleted = await _unitOfWork.Current.Proceso.GetIncludingDeleted(a => a.IsDeleted == true && a.Id == request.Id,cancellationToken);
        
        return procesoSoftDeleted == null 
            ? Result<ProcesoDto>.NotFound("Proceso eliminado no encontrado") 
            : Result<ProcesoDto>.Success(procesoSoftDeleted.MapToDto());
    }
}