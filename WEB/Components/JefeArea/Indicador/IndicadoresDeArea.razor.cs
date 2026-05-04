using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Components.JefeArea.Indicador;
using WEB.Features.Area.Get;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.IndicadorDeArea.GetAll;
using WEB.Features.IndicadorDeArea.Update;

namespace WEB.Components.JefeArea.Indicador;

public partial class IndicadoresDeArea : ComponentBase
{
    private string areaNombre = "Cargando...";
    private string? errorMessage;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            var user = await CurrentUser.GetUserAsync();
            if (user != null)
            {
                var result = await Mediator.Send(new GetAreaByJefeIdRequest(user.Id));
                areaNombre = result.IsSuccess && result.Value != null
                    ? result.Value.Nombre
                    : "No asignada";
            }
            else
            {
                areaNombre = "No asignada";
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            areaNombre = "Error";
            Console.WriteLine($"Error cargando área: {ex.Message}");
        }
    }
}