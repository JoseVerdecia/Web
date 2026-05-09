using Microsoft.EntityFrameworkCore;
using WEB.Core.Mediator;
using WEB.Core.Result;
using WEB.Data;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.GetAll;

public class GetAllIndicadoresByObjetivoAndProcesoHandler 
    : IRequestHandler<GetAllIndicadoresByObjetivoAndProcesoRequest, List<IndicadorDto>>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GetAllIndicadoresByObjetivoAndProcesoHandler(IDbContextFactory<ApplicationDbContext> dbFactory)
        => _dbFactory = dbFactory;

    public async Task<Result<List<IndicadorDto>>> Handle(
        GetAllIndicadoresByObjetivoAndProcesoRequest request, 
        CancellationToken ct)
    {
        await using var context = await _dbFactory.CreateDbContextAsync(ct);

        var indicadores = await context.Indicador
            .Include(i => i.Proceso)
            .Include(i => i.Objetivos)
            .Where(i => i.ProcesoId == request.ProcesoId &&
                        i.Objetivos.Any(o => o.Id == request.ObjetivoId) &&
                        !i.IsDeleted)
            .OrderBy(i => i.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        var dtos = indicadores.Select(i => i.MapToDto()).ToList();
        return Result<List<IndicadorDto>>.Success(dtos);
    }
}