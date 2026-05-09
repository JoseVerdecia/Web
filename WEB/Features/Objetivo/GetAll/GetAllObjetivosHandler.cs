using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Objetivo.Dto;
using WEB.Interfaces;

namespace WEB.Features.Objetivo.GetAll;

public class GetAllObjetivosHandler : IRequestHandler<GetAllObjetivosRequest, PagedResult<ObjetivoDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllObjetivosHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<ObjetivoDto>>> Handle(GetAllObjetivosRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Objetivo.GetPagedAsync(request.Page, request.PageSize,cancellationToken,includeProperties:"Indicadores,Indicadores.Proceso");
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