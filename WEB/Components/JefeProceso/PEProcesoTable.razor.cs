using Microsoft.AspNetCore.Components;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;

namespace WEB.Components.JefeProceso;

public partial class PEProcesoTable : ComponentBase
{
    [Parameter] public int ProcesoId { get; set; }

    private List<IndicadorDto> indicadores = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        StateHasChanged();
        try
        {
            var result = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(ProcesoId));
            indicadores = (result.IsSuccess && result.Value != null)
                ? result.Value.OrderBy(i => i.Id).ToList()
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
}