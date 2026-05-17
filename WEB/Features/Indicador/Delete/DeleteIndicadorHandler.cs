using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Indicador.Delete;

public class DeleteIndicadorHandler : IRequestHandler<DeleteIndicadorRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteCascadeService;

    public DeleteIndicadorHandler(IUnitOfWorkAccessor uow, IDeleteCascadeService deleteCascadeService )
    {
        _uow = uow;
        _deleteCascadeService = deleteCascadeService;
    }

    public async Task<AppResult<Unit>> Handle(DeleteIndicadorRequest request, CancellationToken cancellationToken)
    {
        IndicadorModel? indicador = await _uow.Current.Indicador.GetIncludingDeleted(a => a.Id == request.Id,cancellationToken);

        if (indicador == null) 
            return AppResult<Unit>.NotFound("Indicador no encontrada");

        if (request.Permanent)
        {
            foreach (var ia in indicador.IndicadoresDeArea.ToList())
            {
                if (ia.Notificaciones?.Any() == true)
                {
                    _uow.Current.Notificacion.DeleteRange(ia.Notificaciones);
                }
            }
            
            _uow.Current.IndicadorDeArea.DeleteRange(indicador.IndicadoresDeArea);
            
            _uow.Current.Indicador.Delete(indicador);
        }
        else
        {
            if (indicador.IsDeleted) 
                return AppResult<Unit>.Fail("El indicador ya fue eliminado anteriormente.");
            
            await _deleteCascadeService.SoftDeleteIndicador(indicador);
        }
        await _uow.Current.SaveAsync();
       
        return AppResult<Unit>.Success(Unit.Value);
    }
}