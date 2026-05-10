using Hangfire;
using Hangfire.Storage;
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

    private List<TrabajoProgramado> trabajosProgramados = new();

    private DateTime? FechaInicioCombinada => Combinar(FechaInicio, HoraInicio);
    private DateTime? FechaFinCombinada => Combinar(FechaFin, HoraFin);

    protected override void OnInitialized()
    {
        EvaluationPeriod.OnChanged += Refresh;
        CargarTrabajosProgramados();
    }

    private DateTime? Combinar(DateTime? fecha, DateTime? hora)
    {
        if (fecha == null || hora == null) return null;
        return DateTime.SpecifyKind(
            new DateTime(fecha.Value.Year, fecha.Value.Month, fecha.Value.Day,
                hora.Value.Hour, hora.Value.Minute, 0),
            DateTimeKind.Local);
    }

    private bool FechaPasada(DateTime date) => date.Date < DateTime.Today;

    private bool EsPeriodoValido()
    {
        var inicio = FechaInicioCombinada;
        var fin = FechaFinCombinada;
        return inicio.HasValue && fin.HasValue 
                               && inicio.Value > DateTime.Now   
                               && fin.Value > inicio.Value;
    }

    private async Task ProgramarPeriodoAsync()
    {
        var inicio = FechaInicioCombinada!.Value;
        var fin = FechaFinCombinada!.Value;
        
        DateTime inicioUtc = inicio.ToUniversalTime();
        DateTime finUtc = fin.ToUniversalTime();

        Programando = true;
        Mensaje = string.Empty;
        try
        {
            if (!string.IsNullOrEmpty(JobIdInicio)) BackgroundJobClient.Delete(JobIdInicio);
            if (!string.IsNullOrEmpty(JobIdFin)) BackgroundJobClient.Delete(JobIdFin);

            JobIdInicio = BackgroundJobClient.Schedule<EvaluationPeriodService>(
                s => s.IniciarEvaluacion(),
                inicioUtc - DateTime.UtcNow);

            JobIdFin = BackgroundJobClient.Schedule<EvaluationPeriodService>(
                s => s.FinalizarEvaluacion(),
                finUtc - DateTime.UtcNow);

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
            CargarTrabajosProgramados();
        }
    }

    private void CargarTrabajosProgramados()
    {
        trabajosProgramados.Clear();
        
        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue); 

        foreach (var job in scheduledJobs)
        {
           
            if (job.Value.Job?.Method == null) continue;

            var methodName = job.Value.Job.Method.Name;
            if (methodName == nameof(EvaluationPeriodService.IniciarEvaluacion) ||
                methodName == nameof(EvaluationPeriodService.FinalizarEvaluacion))
            {
                trabajosProgramados.Add(new TrabajoProgramado
                {
                    JobId = job.Key,
                    Tipo = methodName == nameof(EvaluationPeriodService.IniciarEvaluacion) ? "Inicio" : "Fin",
                    FechaProgramada = job.Value.EnqueueAt.ToLocalTime()
                });
            }
        }
    }

    private async Task CancelarTrabajoAsync(string jobId)
    {
        try
        {
            BackgroundJobClient.Delete(jobId);
            CargarTrabajosProgramados();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cancelar: {ex.Message}";
            EsError = true;
        }
    }

    private async void Refresh()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        EvaluationPeriod.OnChanged -= Refresh;
    }

    private class TrabajoProgramado
    {
        public string JobId { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Inicio" o "Fin"
        public DateTime FechaProgramada { get; set; }
    }
}