using System.Linq.Expressions;
using System.Security.Claims;
using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.IndicadorDeArea.GetAll;

public class GetAllIndicadorDeAreaHandler : IRequestHandler<GetAllIndicadorDeAreaRequest , PagedResult<IndicadorDeAreaDto>>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly ICurrentUser _currentUser; 

    public GetAllIndicadorDeAreaHandler(IUnitOfWorkAccessor uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }


    public async Task<Result<PagedResult<IndicadorDeAreaDto>>> Handle(GetAllIndicadorDeAreaRequest request, CancellationToken cancellationToken)
    {
        var user = _currentUser.User;
        if (user == null) return Result<PagedResult<IndicadorDeAreaDto>>.Fail("Usuario no autenticado");

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        Expression<Func<IndicadorDeAreaModel, bool>>? predicate = null;
        
        bool isAdmin = roles.Contains(AppRoles.Administrador);
        bool isJefeArea = roles.Contains(AppRoles.JefeArea);

        if (isAdmin)
        {
            // Admin no usa filtro
        }
        else if (isJefeArea)
            predicate = ia => ia.Area.JefeAreaId == userId;
        else
            predicate = ia => ia.Indicador.Proceso.JefeProcesoId == userId;

        
        var (items, totalCount) = await _uow.Current.IndicadorDeArea.GetPagedAsync(
            request.Page, 
            request.PageSize, 
            predicate: predicate, 
            includeProperties: "Indicador.Proceso,Area" 
        );

        PagedResult<IndicadorDeAreaDto> pagedResult = new PagedResult<IndicadorDeAreaDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<IndicadorDeAreaDto>>.Success(pagedResult);
    }
}