using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Indicador.Dto;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Objetivo.GetAll;

public class GetAllObjetivosByProcesoHandler : IRequestHandler<GetAllObjetivosByProcesoRequest, List<ObjetivoDto>>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GetAllObjetivosByProcesoHandler(IDbContextFactory<ApplicationDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task<AppResult<List<ObjetivoDto>>> Handle(
        GetAllObjetivosByProcesoRequest request, 
        CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);

        var objetivos = await context.Indicador
            .Where(i => i.ProcesoId == request.ProcesoId && !i.IsDeleted)
            .SelectMany(i => i.Objetivos)
            .Distinct()
            .AsNoTracking()
            .ToListAsync(ct);
        
        var dtos = objetivos
            .OrderBy(o => o.NumeroObjetivo)
            .Select(o => new ObjetivoDto 
            (
                Id : o.Id, 
                Nombre : o.Nombre ,
                NumeroObjetivo : o.NumeroObjetivo, 
                Evaluacion:o.Evaluacion,
                DeleteAt:o.DeletedAt,
                Indicadores:o.Indicadores.Select(i=> new IndicadorSimpleDto
                (
                    Id : i.Id,
                    MetaCumplir : i.MetaCumplir,
                    MetaReal : i.MetaReal,
                    Nombre : i.Nombre,
                    ProcesoNombre : i.Proceso.Nombre,
                    Evaluacion : i.Evaluacion
                ))
              
            ))
            .ToList();

        return AppResult<List<ObjetivoDto>>.Success(dtos);
    }
}
