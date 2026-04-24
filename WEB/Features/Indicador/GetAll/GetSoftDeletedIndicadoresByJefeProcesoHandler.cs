using System.Linq.Expressions;
using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.GetAll;

public class GetSoftDeletedIndicadoresByJefeProcesoHandler 
    : IRequestHandler<GetSoftDeletedIndicadoresByJefeProcesoRequest, PagedResult<IndicadorDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetSoftDeletedIndicadoresByJefeProcesoHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<IndicadorDto>>> Handle(
        GetSoftDeletedIndicadoresByJefeProcesoRequest request, 
        CancellationToken cancellationToken)
    {
        Expression<Func<IndicadorModel, bool>> predicate = i => 
            i.IsDeleted && 
            i.Proceso != null && 
            i.Proceso.JefeProcesoId == request.JefeProcesoId;

        var (items, totalCount) = await _uow.Current.Indicador.GetPagedIncludingDeletedAsync(
            request.Page,
            request.PageSize,
            cancellationToken,
            predicate,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea,IndicadoresDeArea.Area"
        );

        var pagedResult = new PagedResult<IndicadorDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<IndicadorDto>>.Success(pagedResult);
    }
}