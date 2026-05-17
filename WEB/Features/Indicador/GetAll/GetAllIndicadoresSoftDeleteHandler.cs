using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Indicador.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.Indicador.GetAll;

public class GetAllIndicadoresSoftDeleteHandler:IRequestHandler<GetAllIndicadoresSoftDeleteRequest, PagedResult<IndicadorDto>>
{
    private readonly IUnitOfWorkAccessor _uow;
   
    public GetAllIndicadoresSoftDeleteHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }


    public async Task<AppResult<PagedResult<IndicadorDto>>> Handle(GetAllIndicadoresSoftDeleteRequest request, CancellationToken cancellationToken)
    {
        
        var (items, totalCount) = await _uow.Current.Indicador.GetPagedIncludingDeletedAsync(
            request.Page,
            request.PageSize,
            cancellationToken,
            i=>i.IsDeleted,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea,IndicadoresDeArea.Area"
        );

        var pagedResult = new PagedResult<IndicadorDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return AppResult<PagedResult<IndicadorDto>>.Success(pagedResult);
    }
}