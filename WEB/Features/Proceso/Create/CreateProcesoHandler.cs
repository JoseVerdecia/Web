using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Proceso.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Create;

public class CreateProcesoHandler : IRequestHandler<CreateProcesoCommand, ProcesoDto>
{
    private readonly IUnitOfWorkAccessor _uow;
  //  private readonly IHubContext<DashboardHub> _hubContext; 
   
    public CreateProcesoHandler(IUnitOfWorkAccessor uow/*, IHubContext<DashboardHub> hubContext*/)
    {
        _uow = uow;
        
      //  _hubContext = hubContext;
    }

    public async Task<Result<ProcesoDto>> Handle(CreateProcesoCommand command, CancellationToken cancellationToken)
    {
        return await Result<ProcesoModel>.Success(new ProcesoModel 
            { 
                Nombre = command.Nombre,
                Evaluacion = Enums.Evaluacion.NoEvaluado
            })
            .Tap(proceso => _uow.Current.Proceso.Add(proceso))
            .TapAsync(_ => _uow.Current.SaveAsync())
            //.TapInvalidateCacheAsync(_cacheStore,cancellationToken,CacheTags.AllProcesos)
            //.TapAsync(_ => _hubContext.Clients.Group(GroupNames.Administradores).SendAsync("StatsUpdated", cancellationToken))
            .Map(proceso => proceso.MapToDto());
    }
}