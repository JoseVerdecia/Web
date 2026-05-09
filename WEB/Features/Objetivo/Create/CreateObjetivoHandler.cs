using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Objetivo.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Objetivo.Create;

public class CreateObjetivoHandler : IRequestHandler<CreateObjetivoCommand, ObjetivoDto>
{
    private readonly IUnitOfWorkAccessor _uow;
    /*private readonly IOutputCacheStore _cacheStore;
    private readonly IHubContext<DashboardHub> _hubContext; */

    public CreateObjetivoHandler(IUnitOfWorkAccessor uow/*, IHubContext<DashboardHub> hubContext*/)
    {
        _uow = uow;
        /*_cacheStore = cacheStore;
        _hubContext = hubContext;*/
    }

    public async Task<Result<ObjetivoDto>> Handle(CreateObjetivoCommand command, CancellationToken cancellationToken)
    {
        return await Result<ObjetivoModel>.Success(new ObjetivoModel
            {
                Nombre = command.Nombre,
                NumeroObjetivo = command.NumeroObjetivo,
                Evaluacion = Enums.Evaluacion.NoEvaluado
            })
            .Tap(objetivo => _uow.Current.Objetivo.Add(objetivo))
            .TapAsync(_ => _uow.Current.SaveAsync())
            //.TapInvalidateCacheAsync(_cacheStore,cancellationToken,CacheTags.AllObjetivos)
            //.TapAsync(_=> _hubContext.Clients.Group(GroupNames.Administradores).SendAsync("StatsUpdated", cancellationToken))
            .Map(objetivo => objetivo.MapToDto());
    }
}