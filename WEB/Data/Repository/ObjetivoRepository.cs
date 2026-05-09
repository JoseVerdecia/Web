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
    
    
}   