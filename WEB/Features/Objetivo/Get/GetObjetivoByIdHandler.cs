using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Features.Objetivo.Dto;
using WEB.Core.Interfaces;

namespace WEB.Features.Objetivo.Get;

public class GetObjetivoByIdHandler : IRequestHandler<GetObjetivoByIdRequest, ObjetivoDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetObjetivoByIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<AppResult<ObjetivoDto>> Handle(GetObjetivoByIdRequest request, CancellationToken cancellationToken)
    {
        var objetivo = await _uow.Current.Objetivo.Get(o => o.Id == request.Id,cancellationToken,includeProperties:"Indicadores,Indicadores.Proceso");
        
        return objetivo == null 
            ? AppResult<ObjetivoDto>.NotFound("Objetivo no encontrado") 
            : AppResult<ObjetivoDto>.Success(objetivo.MapToDto());
    }
}