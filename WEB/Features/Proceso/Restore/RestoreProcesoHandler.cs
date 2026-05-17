using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Proceso.Restore;

public class RestoreProcesoHandler : IRequestHandler<RestoreProcesoRequest,Unit>
{
    private readonly IUnitOfWorkAccessor _uow;

    public RestoreProcesoHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<Unit>> Handle(RestoreProcesoRequest request, CancellationToken cancellationToken)
    {
        if (request.ProcesoIds == null || !request.ProcesoIds.Any())
            return AppResult<Unit>.Fail("No se proporcionaron procesos para restaurar.");

        List<ProcesoModel> procesos = new List<ProcesoModel>();
        foreach (var id in request.ProcesoIds)
        {
            var proceso = await _uow.Current.Proceso.GetIncludingDeleted(p => p.Id == id, cancellationToken);
            if (proceso == null)
                return AppResult<Unit>.Fail($"Proceso con ID {id} no encontrado.");
            procesos.Add(proceso);
        }

        foreach (var proceso in procesos)
        {
            proceso.IsDeleted = false;
            proceso.DeletedAt = null;
            _uow.Current.Proceso.Update(proceso);
        }

        await _uow.Current.SaveAsync();
        return AppResult<Unit>.Success(Unit.Value);
    }
}