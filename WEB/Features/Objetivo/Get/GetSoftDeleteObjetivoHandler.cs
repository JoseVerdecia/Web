using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Objetivo.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Objetivo.Get;

public class GetSoftDeleteObjetivoHandler:IRequestHandler<GetSoftDeleteObjetivoRequest,ObjetivoDto>
{
    private readonly IUnitOfWorkAccessor _unitOfWork;

    public GetSoftDeleteObjetivoHandler(IUnitOfWorkAccessor unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<ObjetivoDto>> Handle(GetSoftDeleteObjetivoRequest request, CancellationToken cancellationToken)
    {
        ObjetivoModel? objetivoSoftDeleted = await _unitOfWork.Current.Objetivo.GetIncludingDeleted(a => a.IsDeleted == true && a.Id == request.Id,cancellationToken);
        
        return objetivoSoftDeleted == null 
            ? Result<ObjetivoDto>.NotFound("Objetivo eliminado no encontrado") 
            : Result<ObjetivoDto>.Success(objetivoSoftDeleted.MapToDto());
    }
}