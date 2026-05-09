using Hangfire;
using Microsoft.AspNetCore.Components;
using WEB.Core.Services;


namespace WEB.Components.Pages;

public partial class Evaluation : ComponentBase
{
     private DateTime? FechaInicio = DateTime.Today;
    private DateTime? HoraInicio = DateTime.Today.AddHours(9);
    private DateTime? FechaFin = DateTime.Today.AddDays(7);
    private DateTime? HoraFin = DateTime.Today.AddDays(7).AddHours(18);

    private bool Programando;
    private string Mensaje = string.Empty;
    private bool EsError;
    private string? JobIdInicio;
    private string? JobIdFin;

    private DateTime? FechaInicioCombinada => Combinar(FechaInicio, HoraInicio);
    private DateTime? FechaFinCombinada => Combinar(FechaFin, HoraFin);

    private DateTime? Combinar(DateTime? fecha, DateTime? hora)
    {
        if (fecha == null || hora == null) return null;
        return new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day,
                            hora.Value.Hour, hora.Value.Minute, 0);
    }

    private bool FechaPasada(DateTime date) => date.Date < DateTime.Today;

    private bool EsPeriodoValido()
    {
        var inicio = FechaInicioCombinada;
        var fin = FechaFinCombinada;
        return inicio.HasValue && fin.HasValue && inicio.Value > DateTime.Now && fin.Value > inicio.Value;
    }

    private async Task ProgramarPeriodoAsync()
    {
        var inicio = FechaInicioCombinada!.Value;
        var fin = FechaFinCombinada!.Value;

        Programando = true;
        Mensaje = string.Empty;
        try
        {
          
            if (!string.IsNullOrEmpty(JobIdInicio)) BackgroundJobClient.Delete(JobIdInicio);
            if (!string.IsNullOrEmpty(JobIdFin)) BackgroundJobClient.Delete(JobIdFin);

           
            JobIdInicio = BackgroundJobClient.Schedule<EvaluationPeriodService>(
                s => s.IniciarEvaluacion(),
                inicio - DateTime.Now);

           
            JobIdFin = BackgroundJobClient.Schedule<EvaluationPeriodService>(
                s => s.FinalizarEvaluacion(),
                fin - DateTime.Now);

            Mensaje = "Periodo de evaluación programado correctamente.";
            EsError = false;
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al programar: {ex.Message}";
            EsError = true;
        }
        finally
        {
            Programando = false;
        }
    }
    
    protected override void OnInitialized()
    {
        EvaluationPeriod.OnChanged += Refresh;
    }

    private async void Refresh()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        EvaluationPeriod.OnChanged -= Refresh;
    }
}