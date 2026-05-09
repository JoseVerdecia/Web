using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Get;

public class GetSoftDeleteIndicadorHandler:IRequestHandler<GetSoftDeleteIndicadorRequest,IndicadorDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetSoftDeleteIndicadorHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }
    public async Task<Result<IndicadorDto>> Handle(GetSoftDeleteIndicadorRequest request, CancellationToken cancellationToken)
    {
        IndicadorModel? areaSoftDeleted = await _uow.Current.Indicador.GetIncludingDeleted(a => a.IsDeleted == true && a.Id == request.Id,cancellationToken);
        
        return areaSoftDeleted == null 
            ? Result<IndicadorDto>.NotFound("Indicador eliminado no encontrado") 
            : Result<IndicadorDto>.Success(areaSoftDeleted.MapToDto());
    }
}