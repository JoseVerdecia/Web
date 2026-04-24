using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.SignalR;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Area.Dto;
using WEB.Interfaces;

namespace WEB.Features.Area.Update;

public class UpdateAreaHandler : IRequestHandler<UpdateAreaCommand, AreaDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public UpdateAreaHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<AreaDto>> Handle(UpdateAreaCommand command, CancellationToken cancellationToken)
    {
        var area = await _uow.Current.Area.Get(a => a.Id == command.Id,cancellationToken);

        if (area == null)
            return Result<AreaDto>.NotFound("Área no encontrada");

        area.Nombre = command.Nombre;
        area.Tipo = command.Tipo;

        _uow.Current.Area.Update(area);
        await _uow.Current.SaveAsync();


        return Result<AreaDto>.Success(area.MapToDto());
    }
}