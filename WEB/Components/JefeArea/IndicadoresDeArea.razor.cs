using Microsoft.AspNetCore.Components;
using WEB.Features.Area.Get;

namespace WEB.Components.JefeArea;

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