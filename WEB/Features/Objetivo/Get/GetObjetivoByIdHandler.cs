using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data.IRepository;
using WEB.Features.Objetivo.Dto;
using WEB.Interfaces;

namespace WEB.Features.Objetivo.Get;

public class GetObjetivoByIdHandler : IRequestHandler<GetObjetivoByIdRequest, ObjetivoDto>
{
    private readonly IUnitOfWorkAccessor _uow;

    public GetObjetivoByIdHandler(IUnitOfWorkAccessor uow)
    {
        _uow = uow;
    }

    public async Task<Result<ObjetivoDto>> Handle(GetObjetivoByIdRequest request, CancellationToken cancellationToken)
    {
        var objetivo = await _uow.Current.Objetivo.Get(o => o.Id == request.Id,cancellationToken,includeProperties:"Indicadores,Indicadores.Proceso");
        
        return objetivo == null 
            ? Result<ObjetivoDto>.NotFound("Objetivo no encontrado") 
            : Result<ObjetivoDto>.Success(objetivo.MapToDto());
    }
}