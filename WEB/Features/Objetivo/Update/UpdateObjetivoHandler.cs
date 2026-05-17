using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Objetivo.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.Objetivo.Update;

public class UpdateObjetivoHandler : IRequestHandler<UpdateObjetivoCommand, ObjetivoDto>
{
    private readonly IUnitOfWorkAccessor _uow;
 
    public UpdateObjetivoHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<ObjetivoDto>> Handle(UpdateObjetivoCommand command, CancellationToken cancellationToken)
    {
        var objetivo = await _uow.Current.Objetivo.Get(o => o.Id == command.Id,cancellationToken);

        if (objetivo == null)
            return AppResult<ObjetivoDto>.NotFound("Objetivo no encontrado");

        objetivo.Nombre = command.Nombre;
        objetivo.NumeroObjetivo = command.NumeroObjetivo;

        _uow.Current.Objetivo.Update(objetivo);
        await _uow.Current.SaveAsync();
        
        return AppResult<ObjetivoDto>.Success(objetivo.MapToDto());
    }
}