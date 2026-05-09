using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Interfaces;

namespace WEB.Features.Proceso.Delete;

public class DeleteAllProcesoHandler:IRequestHandler<DeleteAllProcesoRequest,Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteService;

    public DeleteAllProcesoHandler(
        IUnitOfWorkAccessor uow,
        IDeleteCascadeService deleteService)
    {
        _uow = uow;
        _deleteService = deleteService;
    }

    public async Task<Result<Unit>> Handle(DeleteAllProcesoRequest request, CancellationToken cancellationToken)
    {
        var procesos = await _uow.Current.Proceso.GetAllIncludingDeleted(cancellationToken);

        var lista = procesos
            .Where(p => request.Permanent || !p.IsDeleted)
            .ToList();

        if (!lista.Any())
            return Result<Unit>.Fail("No hay procesos para eliminar");

        if (request.Permanent)
            _uow.Current.Proceso.DeleteRange(lista);
        else
        {
            foreach (var proceso in lista)
            {
                await _deleteService.SoftDeleteProceso(proceso);
            }
        }

        await _uow.Current.SaveAsync();

        return Result<Unit>.Success(Unit.Value);
    }
}