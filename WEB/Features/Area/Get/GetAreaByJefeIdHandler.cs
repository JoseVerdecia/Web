using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Get;

public class GetAreaByJefeIdHandler : IRequestHandler<GetAreaByJefeIdRequest, AreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAreaByJefeIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<AreaDto>> Handle(
        GetAreaByJefeIdRequest request, 
        CancellationToken cancellationToken)
    {
        AreaModel? area = await _uow.Current.Area.Get(
            p => p.JefeAreaId == request.JefeAreaId,
            cancellationToken);

        if (area == null)
            return AppResult<AreaDto>.NotFound("Area no encontrada");

        return AppResult<AreaDto>.Success(area.MapToDto());
    }
}