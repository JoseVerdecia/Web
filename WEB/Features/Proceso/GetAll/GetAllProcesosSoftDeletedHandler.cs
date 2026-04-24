using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Proceso.Dto;
using WEB.Interfaces;

namespace WEB.Features.Proceso.GetAll;

public class GetAllProcesosSoftDeletedHandler:IRequestHandler<GetAllProcesosSoftDeletedRequest,PagedResult<ProcesoDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAllProcesosSoftDeletedHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<ProcesoDto>>> Handle(GetAllProcesosSoftDeletedRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Current.Proceso.GetPagedIncludingDeletedAsync(
            request.Page,
            request.PageSize,
            cancellationToken,
            p=>p.IsDeleted,
            includeProperties: "JefeProceso,Indicadores" 
        );

        var pagedResult = new PagedResult<ProcesoDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<ProcesoDto>>.Success(pagedResult);
    }
}