using WEB.Models;

namespace WEB.Data.IRepository;

public interface IProcesoRepository:IRepository<ProcesoModel>
{
    void Update(ProcesoModel proceso);
    Task<IEnumerable<ProcesoModel>> GetProcesosByObjetivoAsync(int objetivoId);
    Task<ProcesoModel?> GetProcesoByJefeIdAsync(string userId);
    //Task<List<ProcesoDashboardDto>> GetProcesosWithIndicadoresAsync();
}