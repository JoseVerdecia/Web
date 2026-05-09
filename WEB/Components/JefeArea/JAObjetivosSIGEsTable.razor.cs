using Microsoft.AspNetCore.Components;
using WEB.Common;
using WEB.Enums;
using WEB.Features.Indicador.GetAll;
using WEB.Features.IndicadorDeArea.GetAll;

namespace WEB.Components.JefeArea;

public partial class JAObjetivosSIGEsTable : ComponentBase
{
    [Parameter] public int ObjetivoId { get; set; }
    [Parameter] public int AreaId { get; set; }

    private List<FilaObjetivo> filas = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        isLoading = true;
        try
        {

            var objResult = await Mediator.Send(new GetAllIndicadoresByObjetivoRequest(ObjetivoId));
            if (!objResult.IsSuccess || objResult.Value == null) return;

            var indicadoresObjetivo = objResult.Value.OrderBy(i => i.Id).ToList();
            if (!indicadoresObjetivo.Any()) return;


            var areaResult = await Mediator.Send(new GetAllIndicadoresDeAreaByAreaRequest(AreaId));
            if (!areaResult.IsSuccess || areaResult.Value == null) return;

            var indicadoresArea = areaResult.Value.ToList();

            int numeroObjetivo = indicadoresObjetivo.First().Objetivos?.FirstOrDefault(o => o.Id == ObjetivoId)?.NumeroObjetivo ?? 0;
            int secuencia = 0;
            var gruposPorProceso = indicadoresObjetivo.GroupBy(i => i.Proceso?.Id).OrderBy(g => g.First().Proceso?.Nombre);
            filas = new List<FilaObjetivo>();

            foreach (var grupo in gruposPorProceso)
            {
                var inds = grupo.ToList();
                int rowspan = inds.Count;
                bool primera = true;

                foreach (var ind in inds)
                {
                    secuencia++;
                    string numero = $"{numeroObjetivo}.{secuencia:D2}";
                    
                    var area = indicadoresArea.FirstOrDefault(ia => ia.IndicadorPadreId == ind.Id);
                    string metaCumplir = area?.MetaCumplir ?? "—";
                    string metaReal = area?.MetaReal ?? "—";
                    bool tieneReal = !string.IsNullOrEmpty(area?.MetaReal) && area.MetaReal != "—";
                    decimal? pct = null;
                    if (area != null && area.MetaCumplirDecimal != 0 && area.MetaRealDecimal != 0)
                        pct = (area.MetaRealDecimal / area.MetaCumplirDecimal) * 100;

                    string pctText = pct.HasValue ? $"{pct:F2}%" : "—";
                    string color = "#9C9C9C";
                    if (pct.HasValue)
                    {
                        if (pct > 100) color = "#037036";
                        else if (pct == 100) color = "#05B353";
                        else if (pct >= 80) color = "#f97316";
                        else color = "#dc2626";
                    }

                    filas.Add(new FilaObjetivo
                    {
                        Numero = numero,
                        EsPrimeraDelGrupo = primera,
                        RowspanProceso = rowspan,
                        NombreProceso = ind.Proceso?.Nombre ?? "—",
                        IndicadorId = ind.Id,
                        NombreIndicador = ind.Nombre,
                        MetaCumplir = metaCumplir,
                        MetaReal = metaReal,
                        TieneReal = tieneReal,
                        PorcentajeCumplimientoTexto = pctText,
                        ColorPorcentaje = color,
                        Evaluacion = area?.Evaluacion ?? Evaluacion.NoEvaluado,
                        EsEsencial = ind.Tipo == IndicadorTipo.Escencial
                    });
                    primera = false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en JAObjetivosSIGEsTable: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetEvaluacionClass(Evaluacion ev) => ev switch
    {
        Evaluacion.Sobrecumplido => "ev-sobrecumplido",
        Evaluacion.Cumplido => "ev-cumplido",
        Evaluacion.ParcialmenteCumplido => "ev-parcialmente-cumplido",
        Evaluacion.Incumplido => "ev-incumplido",
        _ => "ev-noevaluado"
    };
}