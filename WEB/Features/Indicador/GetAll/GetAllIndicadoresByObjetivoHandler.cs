using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;

namespace WEB.Features.Indicador.GetAll;

public class GetAllIndicadoresByObjetivoHandler : IRequestHandler<GetAllIndicadoresByObjetivoRequest, List<IndicadorDto>>
{
    private readonly IUnitOfWorkAccessor _uow;
    public GetAllIndicadoresByObjetivoHandler(IUnitOfWorkAccessor uow) => _uow = uow;

    public async Task<Result<List<IndicadorDto>>> Handle(GetAllIndicadoresByObjetivoRequest request, CancellationToken ct)
    {
        var indicadores = await _uow.Current.Indicador.GetAllBy(
            i => i.Objetivos.Any(o => o.Id == request.ObjetivoId),
            includeProperties: "Proceso,Objetivos");

        return Result<List<IndicadorDto>>.Success(indicadores.Select(i => i.MapToDto()).ToList());
    }
}