using System.Globalization;
using Microsoft.AspNetCore.Components;
using WEB.Core.Mediator;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;

namespace WEB.Components.Shared;

public partial class IndicadoresDelProcesoSIGES : ComponentBase
{
    [Parameter] public int ProcesoId { get; set; }
    private CancellationTokenSource _cts = new();
    private List<IndicadorDto> indicadores = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await CargarIndicadores();
    }

    private async Task CargarIndicadores()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            var result = await Mediator.Send(
                new GetAllIndicadoresByProcesoRequest(ProcesoId), _cts.Token);
            indicadores = (result.IsSuccess && result.Value != null)
                ? result.Value.ToList()
                : new List<IndicadorDto>();
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            indicadores = new List<IndicadorDto>();
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    #region Cálculos (exactamente los mismos que en ProcesoIndicadores)
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
        if (!total.HasValue || total.Value == 0 || !real.HasValue)
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

    private static string FormatearNumero(decimal value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private string GetEvaluacionClass(Evaluacion evaluacion) => evaluacion switch
    {
        Evaluacion.Sobrecumplido           => "ev-sobrecumplido",
        Evaluacion.Cumplido                => "ev-cumplido",
        Evaluacion.ParcialmenteCumplido    => "ev-parcial",
        Evaluacion.Incumplido              => "ev-incumplido",
        _                                  => "ev-noevaluado"
    };
    #endregion

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}