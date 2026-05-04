using WEB.Core.Extensions;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Indicador.Dto;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Proceso.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.GetAll;



public class GetAllIndicadoresByProcesoHandler : IRequestHandler<GetAllIndicadoresByProcesoRequest, List<IndicadorDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllIndicadoresByProcesoHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<List<IndicadorDto>>> Handle(
        GetAllIndicadoresByProcesoRequest request,
        CancellationToken cancellationToken)
    {
        IEnumerable<IndicadorModel> indicadores = await _uow.Current.Indicador.GetAllByAsNoTracking(
            i => i.ProcesoId == request.ProcesoId,
            cancellationToken,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea.Area"
        );

        List<IndicadorDto> dtos = indicadores.OrderBy(i=>i.Id).MapToDto().ToList();

        return Result<List<IndicadorDto>>.Success(dtos);
    }
}