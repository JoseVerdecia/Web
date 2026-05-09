using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Area.Dto;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Create;

public class CreateAreaHandler : IRequestHandler<CreateAreaCommand, AreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;


    public CreateAreaHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<AreaDto>> Handle(CreateAreaCommand command, CancellationToken cancellationToken)
    {
        return await Result<AreaModel>.Success(new AreaModel 
            { 
                Nombre = command.Nombre,
                Tipo = command.Tipo
            })
            .Tap(area => _uow.Current.Area.Add(area))
            .TapAsync(_ => _uow.Current.SaveAsync())
            /*.TapAsync(_ =>  _cacheStore.InvalidateEntityCache(CacheTags.AllAreas,CacheTags.AreaById,cancellationToken))
            .TapAsync(_=> _hubContext.Clients.Group(GroupNames.Administradores).SendAsync("StatsUpdated", cancellationToken))*/
            .Map(area => area.MapToDto());
    }
}