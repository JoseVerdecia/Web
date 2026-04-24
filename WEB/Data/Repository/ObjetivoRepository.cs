using WEB.Data.IRepository;
using WEB.Models;

namespace WEB.Data.Repository;

public class ObjetivoRepository : Repository<ObjetivoModel>, IObjetivoRepository
{
    private readonly ApplicationDbContext _context;

    public ObjetivoRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(ObjetivoModel objetivo)
    {
        _context.Update(objetivo);
    }
    
    /*public async Task<List<ObjetivoIndicadorCountDto>> GetIndicadorCountByObjetivoAsync()
    {
      
        var query = from objetivo in _context.Objetivo
            where !objetivo.IsDeleted
            select new ObjetivoIndicadorCountDto
            {
                ObjetivoId = objetivo.Id,
                ObjetivoNombre = objetivo.Nombre,
                IndicadoresCount = objetivo.Indicadores.Count(i => !i.IsDeleted)
            };

        return await query.ToListAsync();
    }*/
    
}   