using Microsoft.EntityFrameworkCore;
using WEB.Data.IRepository;
using WEB.Models;

namespace WEB.Data.Repository;

public class ProcesoRepository : Repository<ProcesoModel>, IProcesoRepository
{
    private readonly ApplicationDbContext _context;

    public ProcesoRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    public void Update(ProcesoModel proceso)
    {
        _context.Update(proceso);
    }
    
    public async Task<IEnumerable<ProcesoModel>> GetProcesosByObjetivoAsync(int objetivoId)
    {
        return await _context.Proceso
            .Where(p => p.Indicadores.Any(i => i.Objetivos.Any(o => o.Id == objetivoId)))
            .ToListAsync();
    }

    public async Task<ProcesoModel?> GetProcesoByJefeIdAsync(string userId)
    {
        return await _context.Proceso.FirstOrDefaultAsync(p => p.JefeProcesoId == userId && !p.IsDeleted);
    }

    /*public async Task<List<ProcesoDashboardDto>> GetProcesosWithIndicadoresAsync()
    {
        var query = from proceso in _context.Proceso
            join indicador in _context.Indicador on proceso.Id equals indicador.ProcesoId into indicadores
            from indicador in indicadores.DefaultIfEmpty()
            where !proceso.IsDeleted
            group indicador by new { proceso.Id, proceso.Nombre, proceso.Evaluacion } into g
            select new ProcesoDashboardDto
            {
                ProcesoId = g.Key.Id,
                ProcesoNombre = g.Key.Nombre,
                IndicadoresCount = g.Count(x => x != null),
                Evaluacion = new EvaluationCountsDto
                {
                    Sobrecumplido = g.Count(x => x != null && x.Evaluacion == EntityEvaluation.Sobrecumplido),
                    Cumplido = g.Count(x => x != null && x.Evaluacion == EntityEvaluation.Cumplido),
                    ParcialmenteCumplido = g.Count(x => x != null && x.Evaluacion == EntityEvaluation.ParcialmenteCumplido),
                    Incumplido = g.Count(x => x != null && x.Evaluacion == EntityEvaluation.Incumplido),
                    NoEvaluado = g.Count(x => x != null && x.Evaluacion == EntityEvaluation.NoEvaluado)
                }
            };

        return await query.ToListAsync();
    }*/

}