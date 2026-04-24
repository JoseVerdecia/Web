
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.IndicadorDeArea.Update;

public class UpdateMetaRealHandler:IRequestHandler<UpdateMetaRealRequest, IndicadorDeAreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public UpdateMetaRealHandler(IUnitOfWorkAccessor uow )
    {
        _uow = uow;
       
    }

    public async Task<Result<IndicadorDeAreaDto>> Handle(UpdateMetaRealRequest request, CancellationToken cancellationToken)
    {
        IndicadorDeAreaModel? indicadorDeArea = await _uow.Current.IndicadorDeArea.Get(ia => ia.Id == request.id,includeProperties:"Indicador,Area" );
        
        if (indicadorDeArea == null)
            return Result<IndicadorDeAreaDto>.NotFound("Indicador de Área no encontrado.");

        var result = EvaluacionHelper.ActualizarMetaReal(indicadorDeArea, request.metaReal);
        if (result.IsFailure)
            return Result<IndicadorDeAreaDto>.Fail(result.Errors);
        
         _uow.Current.IndicadorDeArea.Update(indicadorDeArea); 
        await _uow.Current.SaveAsync();

        var updatedDto = indicadorDeArea.MapToDto();
        return Result<IndicadorDeAreaDto>.Success(updatedDto);
    }
}