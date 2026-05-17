using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Users.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Available;

public class GetAvailableProcesosHandler 
    : IRequestHandler<GetAvailableProcesosRequest, List<AvailableUserDto>>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetAvailableProcesosHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<List<AvailableUserDto>>> Handle(
        GetAvailableProcesosRequest request,
        CancellationToken cancellationToken)
    {
        IEnumerable<ProcesoModel> procesos = await _uow.Current.Proceso.GetAllBy( p => p.JefeProcesoId == null,cancellationToken);

        List<AvailableUserDto> AppResult = procesos.Select(p => new AvailableUserDto(p.Id, p.Nombre)).ToList();

        return AppResult<List<AvailableUserDto>>.Success(AppResult);
    }
}