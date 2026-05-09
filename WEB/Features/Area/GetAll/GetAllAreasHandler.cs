using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Interfaces;

namespace WEB.Features.Area.GetAll;

public class GetAllAreasHandler : IRequestHandler<GetAllAreasRequest, PagedResult<AreaDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllAreasHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<AreaDto>>> Handle(GetAllAreasRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Area.GetPagedAsync(request.Page,request.PageSize,cancellationToken,includeProperties:"JefeArea,IndicadoresDeArea,IndicadoresDeArea.Indicador");
        
        PagedResult<AreaDto> pagedResult = new PagedResult<AreaDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
        return Result<PagedResult<AreaDto>>.Success(pagedResult);
    }
}