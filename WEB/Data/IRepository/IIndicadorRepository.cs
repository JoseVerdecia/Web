using WEB.Models;

namespace WEB.Data.IRepository;

public interface IIndicadorRepository:IRepository<IndicadorModel>
{
    void Update(IndicadorModel indicador);
    Task<IEnumerable<IndicadorModel>> GetByProcesoAsync(int procesoId);
    Task<IEnumerable<IndicadorModel>> GetByObjetivoAsync(int objetivoId);
    Task<IEnumerable<IndicadorModel>> GetByAreaAsync(int areaId);
}