using System.Globalization;
using Microsoft.AspNetCore.Components;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;

namespace WEB.Components.Shared;

public partial class ProcesoSIGEsTable : ComponentBase
{
     [Parameter] public int ProcesoId { get; set; }

    private List<IndicadorDto> indicadores = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadIndicadores();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadIndicadores();
    }

    private async Task LoadIndicadores()
    {
        isLoading = true;
        StateHasChanged();
        try
        {
            var result = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(ProcesoId));
            indicadores = (result.IsSuccess && result.Value != null)
                ? result.Value.ToList()
                : new List<IndicadorDto>();
        }
        catch
        {
            indicadores = new List<IndicadorDto>();
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    #region Cálculos y formateo

    private (string Text, string Color) CalcularPorcentajeCumplimiento(IndicadorDto ind)
    {
        if (ind.MetaCumplirDecimal == 0 || ind.MetaRealDecimal == 0)
            return ("0.00%", "#dc2626");

        var porcentaje = (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100;

        var color = porcentaje switch
        {
            > 100 => "#037036",
            100   => "#05B353",
            >= 80 => "#f97316",
            _     => "#dc2626"
        };

        return ($"{porcentaje:F2}%", color);
    }

    private (string Text, string Color) CalcularPorcentaje(decimal? total, decimal? real)
    {
        if (!total.HasValue || total.Value == 0)
            return ("—", "#9C9C9C");

        if (!real.HasValue)
            return ("—", "#9C9C9C");

        var porcentaje = (real.Value / total.Value) * 100;

        var color = porcentaje switch
        {
            >= 100 => "#037036",
            >= 80  => "#05B353",
            >= 50  => "#f97316",
            _      => "#dc2626"
        };

        return ($"{porcentaje:F2}%", color);
    }

    private string GetEvaluacionClass(Evaluacion evaluacion) => evaluacion switch
    {
        Evaluacion.Sobrecumplido           => "ev-sobrecumplido",
        Evaluacion.Cumplido                => "ev-cumplido",
        Evaluacion.ParcialmenteCumplido    => "ev-parcial",
        Evaluacion.Incumplido              => "ev-incumplido",
        _                                  => "ev-noevaluado"
    };

    private static string FormatearNumero(decimal value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    #endregion
}