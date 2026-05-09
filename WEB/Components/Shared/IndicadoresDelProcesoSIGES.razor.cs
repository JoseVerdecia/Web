using Microsoft.AspNetCore.Components;
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
    
    

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}