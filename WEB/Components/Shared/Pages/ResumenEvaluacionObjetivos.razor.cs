using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Common;
using WEB.Core.Services;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;

namespace WEB.Components.Shared.Pages;

public partial class ResumenEvaluacionObjetivos : ComponentBase
{
    private List<ObjetivoResumenData> datosObjetivos = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            
            var allObjetivos = new List<ObjetivoDto>();
            int page = 1;
            int pageSize = 50;
            int totalCount;
            do
            {
                var result = await Mediator.Send(new GetAllObjetivosRequest(page, pageSize));
                if (result.IsSuccess && result.Value?.Items != null)
                {
                    allObjetivos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    page++;
                }
                else break;
            } while (allObjetivos.Count < totalCount);
            
            var tareas = allObjetivos.Select(async obj =>
            {
                var indResult = await Mediator.Send(new GetAllIndicadoresByObjetivoRequest(obj.Id));
                var indicadores = indResult.IsSuccess && indResult.Value != null
                                  ? indResult.Value.ToList()
                                  : new List<IndicadorDto>();
                return new
                {
                    Objetivo = obj,
                    Indicadores = indicadores
                };
            });

            var resultados = await Task.WhenAll(tareas);

            datosObjetivos = resultados.Select(r =>
            {
                var obj = r.Objetivo;
                var indicadores = r.Indicadores;
                var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
                var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();

                
                var filas = new List<FilaResumen>
                {
                    CrearFila("Indicadores esenciales", esenciales),
                    CrearFila("Indicadores necesarios", necesarios)
                };

                
                var evaluacion = EvaluateObjetivosAndProcesos.Evaluar(
                    indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList());

                return new ObjetivoResumenData
                {
                    NumeroObjetivo = obj.NumeroObjetivo,
                    Filas = filas,
                    EvaluacionObjetivo = evaluacion
                };
            }).OrderBy(o => o.NumeroObjetivo).ToList();
        }
        catch
        {
            datosObjetivos = new();
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

    
    private async Task ExportToExcel()
    {
        var data = await ExportService.ExportResumenEvaluacionObjetivosToExcelAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Resumen_Objetivos.xlsx", Convert.ToBase64String(data));
    }

    private async Task ExportToPdf()
    {
        var data = await ExportPdfService.ExportResumenEvaluacionObjetivosToPdfAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Resumen_Objetivos.pdf", Convert.ToBase64String(data));
    }
}