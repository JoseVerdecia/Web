using Hangfire;
using Microsoft.AspNetCore.Components;
using WEB.Core.Services;


namespace WEB.Components.Pages;

public partial class Evaluation : ComponentBase
{
    [Inject] private IBackgroundJobClient BackgroundJobClient { get; set; } = default!;
    [Inject] private GlobalEvaluationService EvaluationService { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;

    private DateTime? FechaSeleccionada = DateTime.Today;
    private DateTime? HoraSeleccionada = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
    private bool Programando = false;
    private string Mensaje = string.Empty;
    private bool EsError = false;
    private string? JobIdProgramado;
    
    private DateTime? FechaHoraCombinada
    {
        get
        {
            if (!FechaSeleccionada.HasValue || !HoraSeleccionada.HasValue)
                return null;

            return new DateTime(
                FechaSeleccionada.Value.Year,
                FechaSeleccionada.Value.Month,
                FechaSeleccionada.Value.Day,
                HoraSeleccionada.Value.Hour,
                HoraSeleccionada.Value.Minute,
                0);
        }
    }
    
    private bool FechaPasada(DateTime date) => date.Date < DateTime.Today;

    private async Task ProgramarJobAsync()
    {
        var fechaProgramada = FechaHoraCombinada;
        if (fechaProgramada == null)
        {
            MostrarMensaje("Debes seleccionar una fecha y hora válidas.", true);
            return;
        }

        if (fechaProgramada <= DateTime.Now)
        {
            MostrarMensaje("La fecha programada debe ser en el futuro.", true);
            return;
        }

        Programando = true;
        Mensaje = string.Empty;

        try
        {
            var delay = fechaProgramada.Value - DateTime.Now;
            
            JobIdProgramado = BackgroundJob.Schedule(
                () => EvaluationService.EjecutarEvaluacionCompletaAsync(),
                delay);

            MostrarMensaje($"Evaluación programada correctamente para el {fechaProgramada.Value:dd/MM/yyyy HH:mm}.", false);
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error al programar: {ex.Message}", true);
        }
        finally
        {
            Programando = false;
        }
    }

    private void MostrarMensaje(string mensaje, bool esError)
    {
        Mensaje = mensaje;
        EsError = esError;
    }
}