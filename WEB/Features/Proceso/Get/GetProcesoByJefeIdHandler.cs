using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Get;

public class GetProcesoByJefeIdHandler : IRequestHandler<GetProcesoByJefeIdRequest, ProcesoDto?>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetProcesoByJefeIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<ProcesoDto?>> Handle(
        GetProcesoByJefeIdRequest request, 
        CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(
            p => p.JefeProcesoId == request.JefeProcesoId,
            cancellationToken);

        if (proceso == null)
            return Result<ProcesoDto?>.Success(null);

        return Result<ProcesoDto?>.Success(proceso.MapToDto());
    }
}