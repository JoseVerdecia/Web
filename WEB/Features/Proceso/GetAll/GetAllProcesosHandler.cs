using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.Proceso.GetAll;

public class GetAllProcesosHandler : IRequestHandler<GetAllProcesosRequest, PagedResult<ProcesoDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllProcesosHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<PagedResult<ProcesoDto>>> Handle(GetAllProcesosRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Proceso.GetPagedAsync(request.Page, request.PageSize,cancellationToken,includeProperties:"Indicadores,JefeProceso");
        var pagedResult = new PagedResult<ProcesoDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return AppResult<PagedResult<ProcesoDto>>.Success(pagedResult);
    }
}