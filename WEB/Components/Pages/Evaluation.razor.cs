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
    private DateTime? _fechaInicioProgramada;
    private DateTime? _fechaFinProgramada;

    private Periodo? PeriodoActual;

    private DateTime? FechaInicioCombinada => Combinar(FechaInicio, HoraInicio);
    private DateTime? FechaFinCombinada => Combinar(FechaFin, HoraFin);

    protected override void OnInitialized()
    {
        EvaluationPeriod.OnChanged += Refresh;
        CargarPeriodoActual();
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

            _fechaInicioProgramada = inicio;
            _fechaFinProgramada = fin;

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
            CargarPeriodoActual();
        }
    }

    private void CargarPeriodoActual()
    {
        bool estaActivo = EvaluationPeriod.IsActive;

        var monitoringApi = JobStorage.Current.GetMonitoringApi();
        var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue);

        var jobInicio = scheduledJobs.FirstOrDefault(j =>
            j.Value.Job?.Method.Name == nameof(EvaluationPeriodService.IniciarEvaluacion));
        var jobFin = scheduledJobs.FirstOrDefault(j =>
            j.Value.Job?.Method.Name == nameof(EvaluationPeriodService.FinalizarEvaluacion));

        if (!estaActivo && jobInicio.Value == null && jobFin.Value == null)
        {
            PeriodoActual = null;
            return;
        }

        DateTime? inicioMostrar = null;
        DateTime? finMostrar = null;

        if (jobInicio.Value != null)
        {
            inicioMostrar = jobInicio.Value.EnqueueAt.ToLocalTime();
            finMostrar = jobFin.Value?.EnqueueAt.ToLocalTime() ?? _fechaFinProgramada;
        }
        else if (jobFin.Value != null)
        {
            inicioMostrar = _fechaInicioProgramada ?? DateTime.MinValue;
            finMostrar = jobFin.Value.EnqueueAt.ToLocalTime();
        }
        else
        {
            inicioMostrar = _fechaInicioProgramada ?? DateTime.Now;
            finMostrar = _fechaFinProgramada ?? DateTime.Now;
        }

        PeriodoActual = new Periodo
        {
            Inicio = inicioMostrar,
            Fin = finMostrar,
            EstaActivo = estaActivo
        };
    }

    private async Task CancelarPeriodoAsync()
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var jobs = monitoringApi.ScheduledJobs(0, int.MaxValue);

            foreach (var job in jobs)
            {
                var method = job.Value.Job?.Method.Name;
                if (method == nameof(EvaluationPeriodService.IniciarEvaluacion) ||
                    method == nameof(EvaluationPeriodService.FinalizarEvaluacion))
                {
                    BackgroundJobClient.Delete(job.Key);
                }
            }

            if (EvaluationPeriod.IsActive)
            {
                EvaluationPeriod.FinalizarEvaluacion();
            }

            JobIdInicio = null;
            JobIdFin = null;
            _fechaInicioProgramada = null;
            _fechaFinProgramada = null;
            PeriodoActual = null;

            Mensaje = "Periodo cancelado correctamente.";
            EsError = false;
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cancelar: {ex.Message}";
            EsError = true;
        }
        finally
        {
            CargarPeriodoActual();
            await InvokeAsync(StateHasChanged);
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

    private class Periodo
    {
        public DateTime? Inicio { get; set; }
        public DateTime? Fin { get; set; }
        public bool EstaActivo { get; set; }
    }
}