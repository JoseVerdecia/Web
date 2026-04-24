using System.Security.Claims;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Data.IRepository;
using WEB.Features.Indicador.Dto;
using WEB.Interfaces;

namespace WEB.Features.Indicador.Get;

public class GetIndicadorHandler : IRequestHandler<GetIndicadorByIdRequest, IndicadorDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly ICurrentUser _currentUser;
    public GetIndicadorHandler(IUnitOfWorkAccessor uow, ICurrentUser currentUser){
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<IndicadorDto>> Handle(GetIndicadorByIdRequest request, CancellationToken cancellationToken)
    {

        var user = _currentUser.User;
        if (user == null)
            return Result<IndicadorDto>.Fail("Usuario no autenticado");

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        var indicador = await _uow.Current.Indicador.Get(
            i => i.Id == request.Id, 
            cancellationToken,
            includeProperties: "Proceso,Objetivos,IndicadoresDeArea,IndicadoresDeArea.Area"
        );
        
        if (indicador == null)
            return Result<IndicadorDto>.NotFound("Indicador no encontrado");


        bool isAdmin = roles.Contains(AppRoles.Administrador);
        bool isJefeProceso = roles.Contains(AppRoles.JefeProceso);

        if (isAdmin)
        {
            return Result<IndicadorDto>.Success(indicador.MapToDto());
        }
        
        if (isJefeProceso)
        {
            
            if (indicador.Proceso != null && indicador.Proceso.JefeProcesoId == userId)
            {
                return Result<IndicadorDto>.Success(indicador.MapToDto());
            }
            
            return Result<IndicadorDto>.NotFound("Indicador no encontrado");
        }
        
        return Result<IndicadorDto>.Fail("Acceso denegado");
    }
}