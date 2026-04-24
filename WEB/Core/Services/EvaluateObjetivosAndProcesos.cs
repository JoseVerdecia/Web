using WEB.Common;
using WEB.Core.Helpers;
using WEB.Enums;

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
}