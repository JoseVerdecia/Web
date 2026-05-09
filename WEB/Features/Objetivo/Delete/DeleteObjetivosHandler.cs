using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Interfaces;

namespace WEB.Features.Objetivo.Delete;

public class DeleteObjetivosHandler : IRequestHandler<DeleteObjetivosRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;

    public DeleteObjetivosHandler(IUnitOfWorkAccessor uow) => _uow = uow;

    public async Task<Result<Unit>> Handle(DeleteObjetivosRequest request, CancellationToken cancellationToken)
    {
        var ids = request.Ids.ToList();
        if (!ids.Any()) return Result<Unit>.Fail("No se proporcionaron objetivos.");

        var objetivos = await _uow.Current.Objetivo.GetAllByIncludingDeleted(o => ids.Contains(o.Id), cancellationToken);
        if (!objetivos.Any()) return Result<Unit>.NotFound("No se encontraron los objetivos.");

        if (request.Permanent) _uow.Current.Objetivo.DeleteRange(objetivos);
        else objetivos.Where(o => !o.IsDeleted).ToList().ForEach(o => _uow.Current.Objetivo.SoftDelete(o));

        await _uow.Current.SaveAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}