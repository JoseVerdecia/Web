using WEB.Models;

namespace WEB.Interfaces;

public interface IDeleteCascadeService
{
    Task SoftDeleteProceso(ProcesoModel? proceso);

    Task SoftDeleteIndicador(IndicadorModel? indicador);

    Task SoftDeleteObjetivo(ObjetivoModel? objetivo);
    Task HardDeleteObjetivo(ObjetivoModel? objetivo);
    
    Task SoftDeleteArea(AreaModel? area);
 
}