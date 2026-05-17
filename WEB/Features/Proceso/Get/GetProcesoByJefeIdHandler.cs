using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Get;

public class GetProcesoByJefeIdHandler : IRequestHandler<GetProcesoByJefeIdRequest, ProcesoDto?>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetProcesoByJefeIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<ProcesoDto?>> Handle(
        GetProcesoByJefeIdRequest request, 
        CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(
            p => p.JefeProcesoId == request.JefeProcesoId,
            cancellationToken);

        if (proceso == null)
            return AppResult<ProcesoDto?>.Success(null);

        return AppResult<ProcesoDto?>.Success(proceso.MapToDto());
    }
}