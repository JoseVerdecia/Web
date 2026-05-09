using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Common;
using WEB.Core.Services;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Proceso.Get;

namespace WEB.Components.JefeProceso;

public partial class JPResumenEvaluacion : ComponentBase
{
    private ProcesoResumenData? datosProceso;
    private int? procesoId;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = await CurrentUser.GetUserAsync();
            var procesoResult = await Mediator.Send(new GetProcesoByJefeIdRequest(user.Id));
            if (!procesoResult.IsSuccess || procesoResult.Value == null)
            {
                isLoading = false;
                return;
            }

            procesoId = procesoResult.Value.Id;
            var nombreProceso = procesoResult.Value.Nombre;

            
            var indResult = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(procesoId.Value));
            if (!indResult.IsSuccess || indResult.Value == null)
            {
                isLoading = false;
                return;
            }

            var indicadores = indResult.Value.ToList();
            var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
            var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();

            datosProceso = new ProcesoResumenData
            {
                NombreProceso = nombreProceso,
                Filas = new List<FilaResumen>
                {
                    CrearFila("Indicadores esenciales", esenciales),
                    CrearFila("Indicadores necesarios", necesarios)
                },
                EvaluacionProceso = EvaluateObjetivosAndProcesos.Evaluar(
                    indicadores.Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion)).ToList())
            };
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
    
    private async Task ExportToExcel()
    {
        if (procesoId == null) return;
        var data = await ExportService.ExportResumenProcesoJefeToExcelAsync(procesoId.Value);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"Evaluacion_Proceso_{procesoId}.xlsx", Convert.ToBase64String(data));
    }

    private async Task ExportToPdf()
    {
        if (procesoId == null) return;
        var data = await ExportPdfService.ExportEvaluacionProcesoToPdfAsync(procesoId.Value);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"Evaluacion_Proceso_{procesoId}.pdf", Convert.ToBase64String(data));
    }
}