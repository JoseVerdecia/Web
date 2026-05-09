using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Objetivo.Dto;
using WEB.Interfaces;

namespace WEB.Features.Objetivo.GetAll;

public class GetAllObjetivosSoftDeleteHandler:IRequestHandler<GetAllObjetivosSoftDeleteRequest,PagedResult<ObjetivoDto>>
{
    private readonly  IUnitOfWorkAccessor _uow;

    public GetAllObjetivosSoftDeleteHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<ObjetivoDto>>> Handle(GetAllObjetivosSoftDeleteRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Objetivo.GetPagedIncludingDeletedAsync(
            request.Page,
            request.PageSize,
            cancellationToken,
            o=>o.IsDeleted
        );

        var pagedResult = new PagedResult<ObjetivoDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
        
        return Result<PagedResult<ObjetivoDto>>.Success(pagedResult);
    }
}