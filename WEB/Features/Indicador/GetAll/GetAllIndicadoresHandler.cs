using System.Linq.Expressions;
using System.Security.Claims;
using WEB.Common;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.GetAll;

public class GetAllIndicadoresHandler : IRequestHandler<GetAllIndicadoresRequest, PagedResult<IndicadorDto>>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly ICurrentUser _currentUser;

    public GetAllIndicadoresHandler(IUnitOfWorkAccessor uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }


    public async Task<Result<PagedResult<IndicadorDto>>> Handle(GetAllIndicadoresRequest request, CancellationToken cancellationToken)
    {
        ClaimsPrincipal? user = _currentUser.User;
        if (user == null)
            return Result<PagedResult<IndicadorDto>>.Fail("Usuario no autenticado");

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        // predicate = null (Obtiene todos los indicadores)
       Expression<Func<IndicadorModel, bool>>? predicate = null;
       
        if(roles.Contains(AppRoles.Administrador))
        {
           
        }
        else if (roles.Contains(AppRoles.JefeProceso))
        {
            // Jefe de proceso: solo indicadores cuyo proceso le pertenezca
            predicate = i => i.Proceso != null && i.Proceso.JefeProcesoId == userId;
        }
        else
        {
            return Result<PagedResult<IndicadorDto>>.Fail("Acceso denegado");
        }
        
        var (items, totalCount) = await _uow.Current.Indicador.GetPagedAsync(
            request.Page,
            request.PageSize,
            cancellationToken:cancellationToken,
            predicate: predicate,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea.Area"
        );
        
        var paged = new PagedResult<IndicadorDto>
        {
            Items = items.MapToDto().ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
        
        return Result<PagedResult<IndicadorDto>>.Success(paged);
    }
}