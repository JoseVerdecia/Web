using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Core.Interfaces;
using WEB.Models;

namespace WEB.Features.Objetivo.Restore;

public class RestoreObjetivosHandler : IRequestHandler<RestoreObjetivosRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _unitOfWork;

    public RestoreObjetivosHandler(IUnitOfWorkAccessor unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AppResult<Unit>> Handle(RestoreObjetivosRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || !request.Ids.Any())
            return AppResult<Unit>.Fail("Debe seleccionar al menos un objetivo.");

        var objetivos = new List<ObjetivoModel>();
        foreach (var id in request.Ids)
        {
            var objetivo = await _unitOfWork.Current.Objetivo.GetIncludingDeleted(a => a.Id == id, cancellationToken);
            if (objetivo == null)
                return AppResult<Unit>.Fail($"Objetivo con ID {id} no encontrado.");
            objetivos.Add(objetivo);
        }
        
        foreach (var objetivo in objetivos)
        {
            objetivo.IsDeleted = false;
            objetivo.DeletedAt = null;
            _unitOfWork.Current.Objetivo.Update(objetivo); 
        }

        await _unitOfWork.Current.SaveAsync();
        return AppResult<Unit>.Success(Unit.Value);
    }
}