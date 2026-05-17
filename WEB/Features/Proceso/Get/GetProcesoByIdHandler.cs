using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Get;

public class GetProcesoByIdHandler : IRequestHandler<GetProcesoByIdRequest, ProcesoDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetProcesoByIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<ProcesoDto>> Handle(GetProcesoByIdRequest request, CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(p => p.Id == request.Id,cancellationToken,includeProperties:"Indicadores");
        
        return proceso == null 
            ? AppResult<ProcesoDto>.NotFound("Proceso no encontrado") 
            : AppResult<ProcesoDto>.Success(proceso.MapToDto());
    }
}