using Microsoft.EntityFrameworkCore;
using WEB.Data.IRepository;
using WEB.Models;

namespace WEB.Data.Repository;

public class IndicadorRepository:Repository<IndicadorModel>,IIndicadorRepository
{
    private readonly ApplicationDbContext _context;
    public IndicadorRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(IndicadorModel indicador)
    {
        _context.Update(indicador);
    }
    public async Task<IEnumerable<IndicadorModel>> GetByProcesoAsync(int procesoId)
    {
        return await dbSet.Where(i => i.ProcesoId == procesoId).ToListAsync();
    }
    public async Task<IEnumerable<IndicadorModel>> GetByObjetivoAsync(int objetivoId)
    {
        return await dbSet
            .Where(i => i.Objetivos.Any(o => o.Id == objetivoId))
            .ToListAsync();
    }
    public async Task<IEnumerable<IndicadorModel>> GetByAreaAsync(int areaId)
    {
        return await dbSet
            .Where(i => i.IndicadoresDeArea.Any(ida => ida.AreaId == areaId))
            .ToListAsync();
    }

    
}