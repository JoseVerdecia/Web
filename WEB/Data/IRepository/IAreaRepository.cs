using WEB.Models;

namespace WEB.Data.IRepository;

public interface IAreaRepository:IRepository<AreaModel>
{
    void Update(AreaModel area);
  //  Task<List<AreaIndicadorCountDto>> GetIndicadorCountByAreaAsync();
}