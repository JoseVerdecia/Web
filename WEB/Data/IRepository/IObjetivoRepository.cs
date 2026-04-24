using WEB.Models;

namespace WEB.Data.IRepository;

public interface IObjetivoRepository:IRepository<ObjetivoModel>
{
    void Update(ObjetivoModel objetivo);
    //Task<List<ObjetivoIndicadorCountDto>> GetIndicadorCountByObjetivoAsync();
 
}