using System.Security.Claims;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.IndicadorDeArea.Get;

public class GetIndicadorDeAreaHandler:IRequestHandler<GetIndicadorDeAreaRequest,IndicadorDeAreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly ICurrentUser _currentUser;

    public GetIndicadorDeAreaHandler(IUnitOfWorkAccessor uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<AppResult<IndicadorDeAreaDto>> Handle(GetIndicadorDeAreaRequest request, CancellationToken cancellationToken)
    {

        ClaimsPrincipal? user = _currentUser.User;
        if (user == null) return AppResult<IndicadorDeAreaDto>.Fail("Usuario no autenticado");

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        IndicadorDeAreaModel? indicadorDeArea = await _uow.Current.IndicadorDeArea.Get(
            ia => ia.Id == request.id,
            includeProperties: "Indicador.Proceso,Area"
        );

        if (indicadorDeArea == null)
            return AppResult<IndicadorDeAreaDto>.NotFound("Indicador de Área no encontrado");

        bool isAdmin = roles.Contains(AppRoles.Administrador);

        if (isAdmin)
        {
            return AppResult<IndicadorDeAreaDto>.Success(indicadorDeArea.MapToDto());
        }

        if (roles.Contains(AppRoles.JefeProceso))
        {
            if (indicadorDeArea.Indicador?.Proceso?.JefeProcesoId == userId)
            {
                return AppResult<IndicadorDeAreaDto>.Success(indicadorDeArea.MapToDto());
            }
            return AppResult<IndicadorDeAreaDto>.NotFound("Indicador de Área no encontrado");
        }
        
        if (roles.Contains(AppRoles.JefeArea))
        {
            if (indicadorDeArea.Area?.JefeAreaId == userId)
            {
                return AppResult<IndicadorDeAreaDto>.Success(indicadorDeArea.MapToDto());
            }
            return AppResult<IndicadorDeAreaDto>.NotFound("Indicador de Área no encontrado");
        }

        return AppResult<IndicadorDeAreaDto>.Fail("Acceso denegado");
    } 
}
