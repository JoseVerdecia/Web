using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Users.Dto;
using WEB.Interfaces;
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

    public async Task<Result<List<AvailableUserDto>>> Handle(
        GetAvailableProcesosRequest request,
        CancellationToken cancellationToken)
    {
        IEnumerable<ProcesoModel> procesos = await _uow.Current.Proceso.GetAllBy( p => p.JefeProcesoId == null,cancellationToken);

        List<AvailableUserDto> result = procesos.Select(p => new AvailableUserDto(p.Id, p.Nombre)).ToList();

        return Result<List<AvailableUserDto>>.Success(result);
    }
}