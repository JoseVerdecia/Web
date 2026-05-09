using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Common;
using WEB.Core.Services;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.Shared.Pages;

public partial class ResumenEvaluacionProcesos : ComponentBase
{
    private List<ProcesoResumenData> datosProcesos = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var procesos = new List<ProcesoDto>();
            int page = 1;
            int pageSize = 50;
            int totalCount;
            do
            {
                var result = await Mediator.Send(new GetAllProcesosRequest(page, pageSize));
                if (result.IsSuccess && result.Value?.Items != null)
                {
                    procesos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                }
                else break;
                page++;
            } while (procesos.Count < totalCount);
            
            var tareas = procesos.Select(async p =>
            {
                var indResult = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(p.Id));
                var indicadores = indResult.IsSuccess && indResult.Value != null
                                  ? indResult.Value.ToList()
                                  : new List<IndicadorDto>();
                return new
                {
                    Proceso = p,
                    Indicadores = indicadores
                };
            });

            var resultados = (await Task.WhenAll(tareas)).ToList();
            
            datosProcesos = resultados.Select(r =>
            {
                var proceso = r.Proceso;
                var indicadores = r.Indicadores;
                var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
                var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();

                return new ProcesoResumenData
                {
                    NombreProceso = proceso.Nombre,
                    Filas = new List<FilaResumen>
                    {
                        CrearFila("Indicadores esenciales", esenciales),
                        CrearFila("Indicadores necesarios", necesarios)
                    },
                    EvaluacionProceso = EvaluateObjetivosAndProcesos.Evaluar(
                        indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList())
                };
            }).ToList();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private FilaResumen CrearFila(string nombre, List<IndicadorDto> indicadores)
    {
        int total = indicadores.Count;
        int sobre = indicadores.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
        int cumple = indicadores.Count(i => i.Evaluacion == Evaluacion.Cumplido);
        int parcial = indicadores.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
        int incumple = indicadores.Count(i => i.Evaluacion == Evaluacion.Incumplido);
        int noEvaluado = indicadores.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);

        int scPlusC = sobre + cumple;
        int scPlusCPlusPC = sobre + cumple + parcial;

        return new FilaResumen
        {
            Nombre = nombre,
            Total = total,
            Sobrecumplidos = sobre,
            PorcentajeS = total > 0 ? (double)sobre / total * 100 : 0,
            Cumplidos = cumple,
            PorcentajeC = total > 0 ? (double)cumple / total * 100 : 0,
            SCplusC = scPlusC,
            PorcentajeSCplusC = total > 0 ? (double)scPlusC / total * 100 : 0,
            ParcialmenteCumplidos = parcial,
            PorcentajePC = total > 0 ? (double)parcial / total * 100 : 0,
            SCplusCplusPC = scPlusCPlusPC,
            PorcentajeSCplusCplusPC = total > 0 ? (double)scPlusCPlusPC / total * 100 : 0,
            Incumplidos = incumple,
            PorcentajeI = total > 0 ? (double)incumple / total * 100 : 0,
            NoEvaluados = noEvaluado
        };
    }

    private string FormatearPorcentaje(double valor)
    {
        if (valor == 0) return "0,00%";
        return valor.ToString("F2", CultureInfo.InvariantCulture) + "%";
    }

    private class ProcesoResumenData
    {
        public string NombreProceso { get; set; } = "";
        public List<FilaResumen> Filas { get; set; } = new();
        public Evaluacion EvaluacionProceso { get; set; }
    }

    private class FilaResumen
    {
        public string Nombre { get; set; } = "";
        public int Total { get; set; }
        public int Sobrecumplidos { get; set; }
        public double PorcentajeS { get; set; }
        public int Cumplidos { get; set; }
        public double PorcentajeC { get; set; }
        public int SCplusC { get; set; }
        public double PorcentajeSCplusC { get; set; }
        public int ParcialmenteCumplidos { get; set; }
        public double PorcentajePC { get; set; }
        public int SCplusCplusPC { get; set; }
        public double PorcentajeSCplusCplusPC { get; set; }
        public int Incumplidos { get; set; }
        public double PorcentajeI { get; set; }
        public int NoEvaluados { get; set; }
    }
    
    private async Task ExportToExcel()
    {
        var data = await ExportService.ExportResumenEvaluacionProcesosToExcelAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Resumen_Procesos.xlsx", Convert.ToBase64String(data));
    }

    private async Task ExportToPdf()
    {
        var data = await ExportPdfService.ExportResumenEvaluacionProcesosToPdfAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Resumen_Procesos.pdf", Convert.ToBase64String(data));
    }
}