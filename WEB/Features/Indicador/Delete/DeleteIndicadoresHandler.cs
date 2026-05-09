using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Interfaces;

namespace WEB.Features.Indicador.Delete;

public class DeleteIndicadoresHandler : IRequestHandler<DeleteIndicadoresRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteCascadeService;

    public DeleteIndicadoresHandler(IUnitOfWorkAccessor uow, IDeleteCascadeService deleteCascadeService)
    {
        _uow = uow;
        _deleteCascadeService = deleteCascadeService;
    }

    public async Task<Result<Unit>> Handle(DeleteIndicadoresRequest request, CancellationToken cancellationToken)
    {
        var ids = request.Ids.ToList();
        
        if (!ids.Any())
            return Result<Unit>.Fail("No se proporcionaron indicadores para eliminar.");

        var indicadores = await _uow.Current.Indicador.GetAllByIncludingDeleted(
            i => ids.Contains(i.Id), 
            cancellationToken);

        if (!indicadores.Any())
            return Result<Unit>.NotFound("No se encontraron los indicadores especificados.");

        if (request.Permanent)
        {
            _uow.Current.Indicador.DeleteRange(indicadores);
        }
        else
        {
            foreach (var indicador in indicadores)
            {
                if (!indicador.IsDeleted)
                {
                    await _deleteCascadeService.SoftDeleteIndicador(indicador);
                }
            }
        }

        await _uow.Current.SaveAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}