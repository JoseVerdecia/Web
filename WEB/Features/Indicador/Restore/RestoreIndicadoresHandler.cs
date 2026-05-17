using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Restore;

public class RestoreIndicadoresHandler : IRequestHandler<RestoreIndicadoresRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;

    public RestoreIndicadoresHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<Unit>> Handle(RestoreIndicadoresRequest request, CancellationToken cancellationToken)
    {
        if (request.IndicadorIds == null || !request.IndicadorIds.Any())
            return AppResult<Unit>.Fail("No se proporcionaron indicadores para restaurar.");

        var indicadores = new List<IndicadorModel>();
        foreach (var id in request.IndicadorIds)
        {
            var indicador = await _uow.Current.Indicador.GetIncludingDeleted(a => a.Id == id, cancellationToken);
            if (indicador == null)
                return AppResult<Unit>.Fail($"Indicador con ID {id} no encontrado.");
            indicadores.Add(indicador);
        }
        
        foreach (var indicador in indicadores)
        {
            indicador.IsDeleted = false;
            indicador.DeletedAt = null;
            _uow.Current.Indicador.Update(indicador); 
        }

        await _uow.Current.SaveAsync();
        return AppResult<Unit>.Success(Unit.Value);
    }
}