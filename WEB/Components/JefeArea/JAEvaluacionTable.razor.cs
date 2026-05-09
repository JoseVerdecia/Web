using Microsoft.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Services;
using WEB.Enums;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.IndicadorDeArea.GetAll;

namespace WEB.Components.JefeArea;

public partial class JAEvaluacionTable : ComponentBase
{
     [Parameter] public int AreaId { get; set; }

    private bool isLoading = true;
    private string nombreArea = "";
    private Evaluacion evaluacionArea;
    private FilaResumen filaEsenciales, filaNecesarios;

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        isLoading = true;
        try
        {
            var result = await Mediator.Send(new GetAllIndicadoresDeAreaByAreaRequest(AreaId));
            if (result.IsSuccess && result.Value != null)
            {
                var indicadores = result.Value.ToList();
                nombreArea = indicadores.FirstOrDefault()?.AreaNombre ?? "Área";
                
                var esenciales = indicadores.Where(ia => ia.IndicadorPadreTipo == IndicadorTipo.Escencial).ToList();
                var necesarios = indicadores.Where(ia => ia.IndicadorPadreTipo == IndicadorTipo.Necesario).ToList();

                filaEsenciales = CrearFila(esenciales);
                filaNecesarios = CrearFila(necesarios);
                
                var dataEvaluacion = indicadores.Select(ia => new IndicadorEvaluacionData(ia.IndicadorPadreTipo, ia.Evaluacion)).ToList();
                evaluacionArea = EvaluateObjetivosAndProcesos.Evaluar(dataEvaluacion);
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error en CargarDatos: {ex.Message}"); }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private FilaResumen CrearFila(List<IndicadorDeAreaDto> lista)
    {
        int total = lista.Count;
        int sobre = lista.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
        int cumple = lista.Count(i => i.Evaluacion == Evaluacion.Cumplido);
        int parcial = lista.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
        int incumple = lista.Count(i => i.Evaluacion == Evaluacion.Incumplido);
        int noEvaluado = lista.Count(i => i.Evaluacion == Evaluacion.NoEvaluado);

        return new FilaResumen
        {
            Total = total,
            Sobrecumplidos = sobre,
            Cumplidos = cumple,
            ParcialmenteCumplidos = parcial,
            Incumplidos = incumple,
            NoEvaluados = noEvaluado,
            SCplusC = sobre + cumple,
            SCplusCplusPC = sobre + cumple + parcial,
            PorcentajeS = total > 0 ? (double)sobre / total * 100 : 0,
            PorcentajeC = total > 0 ? (double)cumple / total * 100 : 0,
            PorcentajePC = total > 0 ? (double)parcial / total * 100 : 0,
            PorcentajeI = total > 0 ? (double)incumple / total * 100 : 0,
            PorcentajeSCplusC = total > 0 ? (double)(sobre + cumple) / total * 100 : 0,
            PorcentajeSCplusCplusPC = total > 0 ? (double)(sobre + cumple + parcial) / total * 100 : 0
        };
    }

    private string FormatearPorcentaje(double valor) => valor == 0 ? "0,00%" : valor.ToString("F2") + "%";
    
}