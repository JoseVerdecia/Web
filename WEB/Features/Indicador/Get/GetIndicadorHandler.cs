using System.Security.Claims;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Indicador.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Get;

public class GetIndicadorHandler : IRequestHandler<GetIndicadorByIdRequest, IndicadorDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly ICurrentUser _currentUser;
    public GetIndicadorHandler(IUnitOfWorkAccessor uow, ICurrentUser currentUser){
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<AppResult<IndicadorDto>> Handle(GetIndicadorByIdRequest request, CancellationToken cancellationToken)
    {

        var user = _currentUser.User;
        if (user == null)
            return AppResult<IndicadorDto>.Fail("Usuario no autenticado");

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        IndicadorModel? indicador = await _uow.Current.Indicador.Get(
            i => i.Id == request.Id, 
            cancellationToken,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea,IndicadoresDeArea.Area"
        );
        
        if (indicador == null)
            return AppResult<IndicadorDto>.NotFound("Indicador no encontrado");


        bool isAdmin = roles.Contains(AppRoles.Administrador);
        bool isJefeProceso = roles.Contains(AppRoles.JefeProceso);

        if (isAdmin)
        {
            return AppResult<IndicadorDto>.Success(indicador.MapToDto());
        }
        
        if (isJefeProceso)
        {
            
            if (indicador.Proceso != null && indicador.Proceso.JefeProcesoId == userId)
            {
                return AppResult<IndicadorDto>.Success(indicador.MapToDto());
            }
            
            return AppResult<IndicadorDto>.NotFound("Indicador no encontrado");
        }
        
        return AppResult<IndicadorDto>.Fail("Acceso denegado");
    }
}