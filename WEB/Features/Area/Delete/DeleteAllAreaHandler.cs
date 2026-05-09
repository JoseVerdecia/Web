using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Interfaces;

namespace WEB.Features.Area.Delete;

public class DeleteAllAreaHandler:IRequestHandler<DeleteAllAreaRequest,Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteService;

    public DeleteAllAreaHandler(
        IUnitOfWorkAccessor uow,
        IDeleteCascadeService deleteService)
    {
        _uow = uow;
        _deleteService = deleteService;
    }

    public async Task<Result<Unit>> Handle(DeleteAllAreaRequest request, CancellationToken cancellationToken)
    {
        var areas = await _uow.Current.Area.GetAllIncludingDeleted(cancellationToken);

        var lista = areas
            .Where(a => request.Permanent || !a.IsDeleted)
            .ToList();

        if (!lista.Any())
            return Result<Unit>.Fail("No hay áreas para eliminar");

        if (request.Permanent)
            _uow.Current.Area.DeleteRange(lista);
        else
        {
            foreach (var area in lista)
            {
                await _deleteService.SoftDeleteArea(area);
            }
        }
        await _uow.Current.SaveAsync();

        return Result<Unit>.Success(Unit.Value);
    }
}