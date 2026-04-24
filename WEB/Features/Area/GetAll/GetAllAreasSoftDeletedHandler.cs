using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Area.Dto;
using WEB.Interfaces;

namespace WEB.Features.Area.GetAll;

public class GetAllAreasSoftDeletedHandler:IRequestHandler<GetAllAreasSoftDeletedRequest,PagedResult<AreaDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllAreasSoftDeletedHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<AreaDto>>> Handle(GetAllAreasSoftDeletedRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Area.GetPagedIncludingDeletedAsync(
            request.Page,
            request.PageSize,
            cancellationToken,
            a=>a.IsDeleted,
            includeProperties: "JefeArea,IndicadoresDeArea,IndicadoresDeArea.Indicador" 
        );

        var pagedResult = new PagedResult<AreaDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<AreaDto>>.Success(pagedResult);
    }
}