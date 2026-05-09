using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Objetivo.Restore;

public class RestoreObjetivosHandler : IRequestHandler<RestoreObjetivosRequest, Unit>
{
    private readonly IUnitOfWorkAccessor _unitOfWork;

    public RestoreObjetivosHandler(IUnitOfWorkAccessor unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RestoreObjetivosRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || !request.Ids.Any())
            return Result<Unit>.Fail("Debe seleccionar al menos un objetivo.");

        var objetivos = new List<ObjetivoModel>();
        foreach (var id in request.Ids)
        {
            var objetivo = await _unitOfWork.Current.Objetivo.GetIncludingDeleted(a => a.Id == id, cancellationToken);
            if (objetivo == null)
                return Result<Unit>.Fail($"Objetivo con ID {id} no encontrado.");
            objetivos.Add(objetivo);
        }
        
        foreach (var objetivo in objetivos)
        {
            objetivo.IsDeleted = false;
            objetivo.DeletedAt = null;
            _unitOfWork.Current.Objetivo.Update(objetivo); 
        }

        await _unitOfWork.Current.SaveAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}