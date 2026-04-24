using Microsoft.AspNetCore.OutputCaching;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Features.Objetivo.Delete;

public class DeleteAllObjetivoHandler:IRequestHandler<DeleteAllObjetivoRequest,Unit>
{
    private readonly IUnitOfWorkAccessor _uow;
    private readonly IDeleteCascadeService _deleteService;

    public DeleteAllObjetivoHandler(
        IUnitOfWorkAccessor uow,
        IDeleteCascadeService deleteService)
    {
        _uow = uow;
        _deleteService = deleteService;
    }
    
    public async Task<Result<Unit>> Handle(DeleteAllObjetivoRequest request, CancellationToken cancellationToken)
    {
        var objetivos = await _uow.Current.Objetivo.GetAllIncludingDeleted(cancellationToken);

        var lista = objetivos.Where(o => request.Permanent || !o.IsDeleted).ToList();

        if (!lista.Any())
            return Result<Unit>.Fail("No hay objetivos para eliminar");

        foreach (var objetivo in lista)
        {
            if (request.Permanent)
                await _deleteService.HardDeleteObjetivo(objetivo);
            else
                await _deleteService.SoftDeleteObjetivo(objetivo);
        }

        await _uow.Current.SaveAsync();

        return Result<Unit>.Success(Unit.Value);
    }
}