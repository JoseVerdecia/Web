using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;

namespace WEB.Features.Indicador.Get;

public class GetIndicadoresByProcesoHandler : IRequestHandler<GetIndicadoresByProcesoRequest, PagedResult<IndicadorDto>>
{
    private readonly IUnitOfWorkAccessor _uow;
    public GetIndicadoresByProcesoHandler(IUnitOfWorkAccessor uow) => _uow = uow;

    public async Task<Result<PagedResult<IndicadorDto>>> Handle(GetIndicadoresByProcesoRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Indicador.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken:cancellationToken,
            predicate: i => i.ProcesoId == request.ProcesoId && !i.IsDeleted,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea.Area");

        var dto = items.Select(i => i.MapToDto()).ToList();
        return Result<PagedResult<IndicadorDto>>.Success(new PagedResult<IndicadorDto>
        {
            Items = dto,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}