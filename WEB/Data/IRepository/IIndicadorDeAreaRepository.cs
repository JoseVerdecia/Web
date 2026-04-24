using WEB.Models;

namespace WEB.Data.IRepository;

public interface IIndicadorDeAreaRepository : IRepository<IndicadorDeAreaModel>
{
    void Update(IndicadorDeAreaModel model);
}