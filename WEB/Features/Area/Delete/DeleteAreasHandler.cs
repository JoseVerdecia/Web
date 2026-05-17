using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;

namespace WEB.Features.Area.Delete;

public class DeleteAreasHandler : IRequestHandler<DeleteAreasRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;

    public DeleteAreasHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<Unit>> Handle(DeleteAreasRequest request, CancellationToken cancellationToken)
    {
        var ids = request.Ids.ToList();
        if (!ids.Any()) return AppResult<Unit>.Fail("No se proporcionaron áreas.");

        var areas = await _uow.Current.Area.GetAllByIncludingDeleted(a => ids.Contains(a.Id), cancellationToken);
        if (!areas.Any()) return AppResult<Unit>.NotFound("No se encontraron las áreas.");

        if (request.Permanent)
        {
            _uow.Current.Area.DeleteRange(areas);
        }
        else
        {
            foreach (var area in areas)
            {
                if (!area.IsDeleted) _uow.Current.Area.SoftDelete(area);
            }
        }

        await _uow.Current.SaveAsync();
        return AppResult<Unit>.Success(Unit.Value);
    }
}