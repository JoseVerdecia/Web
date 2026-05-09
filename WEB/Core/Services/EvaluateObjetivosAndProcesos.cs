using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Helpers;
using WEB.Enums;
using WEB.Models;

namespace WEB.Core.Services;

public static class EvaluateObjetivosAndProcesos
{
    public static Evaluacion Evaluar(IEnumerable<IndicadorEvaluacionData> indicadores)
    {
        List<IndicadorEvaluacionData> lista = indicadores.ToList();

        if (!lista.Any())
            return Evaluacion.NoEvaluado;

        List<IndicadorEvaluacionData> esenciales =
            lista.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();

        List<IndicadorEvaluacionData> necesarios =
            lista.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();

        return EvaluarGrupo(esenciales, necesarios);
    }

    private static Evaluacion EvaluarGrupo(
        List<IndicadorEvaluacionData> esenciales,
        List<IndicadorEvaluacionData> necesarios)
    {
        var e = EvaluacionHelper.CalcularPorcentajes(esenciales);
        var n = EvaluacionHelper.CalcularPorcentajes(necesarios);

        if (EvaluacionHelper.EsSobrecumplido(e, n))
            return Evaluacion.Sobrecumplido;

        if (EvaluacionHelper.EsCumplido(e, n))
            return Evaluacion.Cumplido;

        if (EvaluacionHelper.EsParcial(e, n))
            return Evaluacion.ParcialmenteCumplido;

        return Evaluacion.Incumplido;
    }
    
    public static EvaluacionProcesoData CalcularEvaluacionProceso(ProcesoModel proceso)
    {
        var indicadores = proceso.Indicadores;
        var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
        var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();
    
        EvaluacionProcesoRow CrearFila(string nombre, List<IndicadorModel> lista)
        {
            int total = lista.Count;
            int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
            int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
            int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
            int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
            return new EvaluacionProcesoRow
            {
                Nombre = nombre,
                Total = total,
                Sobrecumplidos = sobre,
                Cumplidos = cumple,
                ParcialmenteCumplidos = parcial,
                Incumplidos = incumple,
                PorcentajeS = total > 0 ? (double)sobre / total * 100 : 0,
                PorcentajeC = total > 0 ? (double)cumple / total * 100 : 0,
                PorcentajePC = total > 0 ? (double)parcial / total * 100 : 0,
                PorcentajeI = total > 0 ? (double)incumple / total * 100 : 0
            };
        }
    
        var filaEsenciales = CrearFila("Esenciales", esenciales);
        var filaNecesarios = CrearFila("Necesarios", necesarios);
    
        int totalTodos = indicadores.Count;
        int sobreTodos = filaEsenciales.Sobrecumplidos + filaNecesarios.Sobrecumplidos;
        int cumpleTodos = filaEsenciales.Cumplidos + filaNecesarios.Cumplidos;
        int parcialTodos = filaEsenciales.ParcialmenteCumplidos + filaNecesarios.ParcialmenteCumplidos;
        int incumpleTodos = filaEsenciales.Incumplidos + filaNecesarios.Incumplidos;
    
        var filaTotales = new EvaluacionProcesoRow
        {
            Nombre = "Totales",
            Total = totalTodos,
            Sobrecumplidos = sobreTodos,
            Cumplidos = cumpleTodos,
            ParcialmenteCumplidos = parcialTodos,
            Incumplidos = incumpleTodos,
            PorcentajeS = 0, 
            PorcentajeC = 0,
            PorcentajePC = 0,
            PorcentajeI = 0
        };
    
      
        var dataEvaluacion = indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList();
        var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(dataEvaluacion); 
    
        return new EvaluacionProcesoData
        {
            NombreProceso = proceso.Nombre,
            Filas = new List<EvaluacionProcesoRow> { filaEsenciales, filaNecesarios, filaTotales },
            Evaluacion = evaluacion.GetDisplayName() 
        };
    }
}