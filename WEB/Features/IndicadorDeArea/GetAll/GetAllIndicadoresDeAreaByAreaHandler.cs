using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.IndicadorDeArea.GetAll;

public class GetAllIndicadoresDeAreaByAreaHandler : IRequestHandler<GetAllIndicadoresDeAreaByAreaRequest, List<IndicadorDeAreaDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllIndicadoresDeAreaByAreaHandler(IUnitOfWorkAccessor uow) => _uow = uow;

    public async Task<AppResult<List<IndicadorDeAreaDto>>> Handle(GetAllIndicadoresDeAreaByAreaRequest request, CancellationToken ct)
    {
        var items = await _uow.Current.IndicadorDeArea.GetAllBy(
            ia => ia.AreaId == request.AreaId,
            includeProperties: "Indicador,Area,Indicador.Proceso"
        );
        return AppResult<List<IndicadorDeAreaDto>>.Success(items.Select(ia => ia.MapToDto()).ToList());
    }
}