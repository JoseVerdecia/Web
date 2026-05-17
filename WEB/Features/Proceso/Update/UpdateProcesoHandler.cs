using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Update;

public class UpdateProcesoHandler : IRequestHandler<UpdateProcesoCommand, ProcesoDto>
{
    private readonly IUnitOfWorkAccessor _uow;
   // private readonly IHubContext<DashboardHub> _hubContext; 
    
    public UpdateProcesoHandler(IUnitOfWorkAccessor uow/*, IOutputCacheStore cacheStore*//*, IHubContext<DashboardHub> hubContext*/)
    {
        _uow = uow;
       // _cacheStore = cacheStore;
      //  _hubContext = hubContext;
    }

    public async Task<AppResult<ProcesoDto>> Handle(UpdateProcesoCommand command, CancellationToken cancellationToken)
    {
        ProcesoModel? proceso = await _uow.Current.Proceso.Get(p => p.Id == command.Id,cancellationToken);

        if (proceso == null)
            return AppResult<ProcesoDto>.NotFound("Proceso no encontrado");

        proceso.Nombre = command.Nombre;

        _uow.Current.Proceso.Update(proceso);
        await _uow.Current.SaveAsync();

      //  await _cacheStore.InvalidateEntityCache(CacheTags.ProcesoById,CacheTags.AllProcesos);
       // await _hubContext.Clients.Group(GroupNames.Administradores).SendAsync("StatsUpdated", cancellationToken);
        return AppResult<ProcesoDto>.Success(proceso.MapToDto());
    }
}