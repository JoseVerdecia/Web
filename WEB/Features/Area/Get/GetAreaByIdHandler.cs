using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Get;

public class GetAreaByIdHandler : IRequestHandler<GetAreaByIdRequest, AreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAreaByIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<AreaDto>> Handle(GetAreaByIdRequest request, CancellationToken cancellationToken)
    {
        AreaModel? area = await _uow.Current.Area.Get(a => a.Id == request.Id,cancellationToken,includeProperties:"IndicadoresDeArea,IndicadoresDeArea.Indicador");
        
        return area == null 
            ? AppResult<AreaDto>.NotFound("Área no encontrada") 
            : AppResult<AreaDto>.Success(area.MapToDto());
    }
}