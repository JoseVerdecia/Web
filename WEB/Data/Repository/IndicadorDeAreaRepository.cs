using WEB.Data.IRepository;
using WEB.Models;

namespace WEB.Data.Repository;

public class IndicadorDeAreaRepository:Repository<IndicadorDeAreaModel>,IIndicadorDeAreaRepository
{
    private readonly ApplicationDbContext _context;
    public IndicadorDeAreaRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
    
    public void Update(IndicadorDeAreaModel model)
    {
        _context.IndicadorDeArea.Update(model);
    }
}