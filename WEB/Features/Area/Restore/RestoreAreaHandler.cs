using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Area.Restore;

public class RestoreAreaHandler : IRequestHandler<RestoreAreaRequest,Unit>
{
    private readonly IUnitOfWorkAccessor _uow;

    public RestoreAreaHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<Unit>> Handle(RestoreAreaRequest request, CancellationToken cancellationToken)
    {
        if (request.AreaIds == null || !request.AreaIds.Any())
            return Result<Unit>.Fail("No se proporcionaron áreas para restaurar.");

        var areas = new List<AreaModel>();
        foreach (var id in request.AreaIds)
        {
            var area = await _uow.Current.Area.GetIncludingDeleted(a => a.Id == id, cancellationToken);
            if (area == null)
                return Result<Unit>.Fail($"Área con ID {id} no encontrada.");
            areas.Add(area);
        }
        
        foreach (var area in areas)
        {
            area.IsDeleted = false;
            area.DeletedAt = null;
            _uow.Current.Area.Update(area); 
        }

        await _uow.Current.SaveAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}