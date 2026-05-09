using Microsoft.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Data;
using WEB.Enums;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.IndicadorDeArea.Get;
using WEB.Features.IndicadorDeArea.Update;

namespace WEB.Components.JefeProceso;

public partial class JPEvaluarIndicadorDeArea : ComponentBase
{
    [Parameter] public int Id { get; set; }

    private IndicadorDeAreaDto? area;
    private bool isLoading = true;
    private string evaluacionSeleccionada = "";
    private List<SelectOption<string?>> evaluaciones = new();
    private string mensaje = "";
    private string error = "";

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
        evaluaciones = Enum.GetValues<Evaluacion>()
            .Select(e => new SelectOption<string?>
            {
                Value = e.ToString(),
                Text = e.GetDisplayName()
            })
            .ToList();
    }
    
    private async Task Regresar()
    {
        if (area == null) return;

        string url;
        var user = await CurrentUser.GetUserAsync();
        if (user != null && CurrentUser.IsInRole(AppRoles.JefeProceso))
        {
            url = $"/indicadores/jefeProceso/{area.IndicadorPadreId}/areas";
        }
        else
        {
            url = $"/indicadores/{area.IndicadorPadreId}/areas";
        }

        NavigationManager.NavigateTo(url);
    }
    private async Task CargarDatos()
    {
        var result = await Mediator.Send(new GetIndicadorDeAreaRequest(Id));
        if (result.IsSuccess && result.Value != null)
        {
            area = result.Value;
            evaluacionSeleccionada = area.Evaluacion.ToString();
        }
        isLoading = false;
    }

    private async Task GuardarCambios()
    {
        if (area == null) return;

        if (!Enum.TryParse<Evaluacion>(evaluacionSeleccionada, out var nuevaEvaluacion))
        {
            error = "Seleccione una evaluación válida.";
            return;
        }

        var request = new UpdateEvaluacionIndicadorDeAreaRequest(Id, nuevaEvaluacion, null);
        var result = await Mediator.Send(request);
        if (result.IsSuccess)
        {
            area = result.Value;
            mensaje = "Evaluación actualizada correctamente.";
            error = "";
        }
        else
        {
            error = string.Join(", ", result.Errors.Select(e => e.Message));
        }

        await Regresar();
    }
    
}