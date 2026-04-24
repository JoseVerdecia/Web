using WEB.Data.IRepository;
using WEB.Models;

namespace WEB.Data.Repository;

public class AreaRepository : Repository<AreaModel>, IAreaRepository
{
    private readonly ApplicationDbContext _context;

    public AreaRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    public void Update(AreaModel area)
    {
        _context.Update(area);
    }
    
    /*public async Task<List<AreaIndicadorCountDto>> GetIndicadorCountByAreaAsync()
    {
        var query = from area in _context.Area
            join indArea in _context.IndicadorDeArea on area.Id equals indArea.AreaId into joined
            from indArea in joined.DefaultIfEmpty()
            where !area.IsDeleted
            group indArea by new { area.Id, area.Nombre } into g
            select new AreaIndicadorCountDto
            {
                AreaId = g.Key.Id,
                AreaNombre = g.Key.Nombre,
                IndicadoresCount = g.Count(x => x != null)
            };

        return await query.ToListAsync();
    }*/
}