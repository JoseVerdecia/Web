using Microsoft.AspNetCore.Components;
using WEB.Common;
using WEB.Enums;
using WEB.Features.Indicador.GetAll;

namespace WEB.Components.Shared;

public partial class ObjetivoSIGEsTable : ComponentBase
{
    [Parameter] public int ObjetivoId { get; set; }

    private List<FilaObjetivo> filas = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        isLoading = true;
        try
        {
            var result = await Mediator.Send(new GetAllIndicadoresByObjetivoRequest(ObjetivoId));
            if (result.IsSuccess && result.Value != null)
            {
                var indicadores = result.Value.OrderBy(i => i.Id).ToList();
                int numeroObjetivo = indicadores.FirstOrDefault()?.Objetivos.FirstOrDefault(o => o.Id == ObjetivoId)?.NumeroObjetivo ?? 0;
                int secuencia = 0;
                var gruposPorProceso = indicadores.GroupBy(i => i.Proceso?.Id).OrderBy(g => g.First().Proceso?.Nombre);
                filas = new List<FilaObjetivo>();

                foreach (var grupo in gruposPorProceso)
                {
                    var indicadoresDelProceso = grupo.ToList();
                    int rowspan = indicadoresDelProceso.Count;
                    bool primera = true;

                    foreach (var ind in indicadoresDelProceso)
                    {
                        secuencia++;
                        string numero = $"{numeroObjetivo}.{secuencia:D2}";
                        decimal? pct = ind.MetaCumplirDecimal != 0 ? (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100 : 0;
                        string pctText = ind.MetaRealDecimal != 0 ? $"{pct:F2}%" : "—";
                        string color;
                        if (!ind.MetaRealDecimal.HasValue || pct == 0) color = "#9C9C9C";
                        else if (pct > 100) color = "#037036";
                        else if (pct == 100) color = "#05B353";
                        else if (pct >= 80) color = "#f97316";
                        else color = "#dc2626";

                        filas.Add(new FilaObjetivo
                        {
                            Numero = numero,
                            EsPrimeraDelGrupo = primera,
                            RowspanProceso = rowspan,
                            NombreProceso = ind.Proceso?.Nombre ?? "—",
                            IndicadorId = ind.Id,
                            NombreIndicador = ind.Nombre,
                            MetaCumplir = ind.MetaCumplir,
                            MetaReal = ind.MetaReal,
                            TieneReal = ind.MetaRealDecimal.HasValue && ind.MetaRealDecimal > 0,
                            PorcentajeCumplimiento = pct,
                            PorcentajeCumplimientoTexto = pctText,
                            ColorPorcentaje = color,
                            Evaluacion = ind.Evaluacion,
                            EsEsencial = ind.Tipo == IndicadorTipo.Escencial
                        });
                        primera = false;
                    }
                }
            }
        }
        catch
        {
            filas = new();
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