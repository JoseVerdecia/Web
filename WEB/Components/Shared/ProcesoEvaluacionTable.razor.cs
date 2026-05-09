using Microsoft.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Services;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;

namespace WEB.Components.Shared;

public partial class ProcesoEvaluacionTable : ComponentBase
{
    [Parameter] public int ProcesoId { get; set; }
    
    private List<FilaEvaluacion> Filas = new();
    private Evaluacion evaluacionProceso = Evaluacion.NoEvaluado;

    protected override async Task OnParametersSetAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        var result = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(ProcesoId));
        if (!result.IsSuccess || result.Value == null)
        {
            Filas = new();
            evaluacionProceso = Evaluacion.NoEvaluado;
            return;
        }

        var indicadores = result.Value.ToList();
        
        var dataEvaluacion = indicadores
            .Select(i => new IndicadorEvaluacionData(i.Tipo, i.Evaluacion))
            .ToList();

        evaluacionProceso = EvaluateObjetivosAndProcesos.Evaluar(dataEvaluacion);
        
        var esenciales = indicadores.Where(i => i.Tipo == IndicadorTipo.Escencial).ToList();
        var necesarios = indicadores.Where(i => i.Tipo == IndicadorTipo.Necesario).ToList();
        
        Filas = new List<FilaEvaluacion>
        {
            CrearFila("Esenciales", esenciales),
            CrearFila("Necesarios", necesarios),
            CrearFilaTotal("Totales", indicadores)
        };
    }

    private FilaEvaluacion CrearFila(string nombre, List<IndicadorDto> indicadores)
    {
        int total = indicadores.Count;
        int sobre = indicadores.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
        int cumple = indicadores.Count(i => i.Evaluacion == Evaluacion.Cumplido);
        int parcial = indicadores.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
        int incumple = indicadores.Count(i => i.Evaluacion == Evaluacion.Incumplido);

        return new FilaEvaluacion
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

    private FilaEvaluacion CrearFilaTotal(string nombre, List<IndicadorDto> todos)
    {
        int total = todos.Count;
        int sobre = todos.Count(i => i.Evaluacion == Evaluacion.Sobrecumplido);
        int cumple = todos.Count(i => i.Evaluacion == Evaluacion.Cumplido);
        int parcial = todos.Count(i => i.Evaluacion == Evaluacion.ParcialmenteCumplido);
        int incumple = todos.Count(i => i.Evaluacion == Evaluacion.Incumplido);

        return new FilaEvaluacion
        {
            Nombre = nombre,
            Total = total,
            Sobrecumplidos = sobre,
            Cumplidos = cumple,
            ParcialmenteCumplidos = parcial,
            Incumplidos = incumple,
            PorcentajeS = 0,
            PorcentajeC = 0,
            PorcentajePC = 0,
            PorcentajeI = 0
        };
    }
    

   
}