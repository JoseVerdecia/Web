using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;

namespace WEB.Components.Shared;

public partial class ProcesosDelObjetivoTable : ComponentBase
{
    [Parameter] public int ProcesoId { get; set; }

    private List<ObjetivoDto> objetivos = new();
    private Dictionary<int, List<IndicadorDto>> indicadoresPorObjetivo = new();
    private HashSet<int> loadingIndicadores = new();
    private bool isLoadingObjetivos = true;
    private string activeTabId = "";
    private CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadObjetivos();
    }

    protected override async Task OnParametersSetAsync()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        indicadoresPorObjetivo.Clear();
        loadingIndicadores.Clear();
        activeTabId = "";
        await LoadObjetivos();
    }

    private async Task LoadObjetivos()
    {
        isLoadingObjetivos = true;
        StateHasChanged();
        try
        {
            const int pageSize = 100;
            int currentPage = 1;
            int totalCount = 0;
            var allObjetivos = new List<ObjetivoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllObjetivosRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value?.Items != null)
                {
                    allObjetivos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allObjetivos.Count < totalCount && !_cts.IsCancellationRequested);

            objetivos = allObjetivos.OrderBy(o => o.NumeroObjetivo).ToList();

            if (objetivos.Any())
            {
                activeTabId = $"tab-objetivo-{objetivos.First().Id}";
                await LoadIndicadoresParaObjetivo(objetivos.First().Id);
            }
        }
        catch (OperationCanceledException) { }
        catch { }
        finally
        {
            isLoadingObjetivos = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == null) return;
        var idStr = tab.Id.Replace("tab-objetivo-", "");
        if (!int.TryParse(idStr, out int objetivoId)) return;

        if (!indicadoresPorObjetivo.ContainsKey(objetivoId) && !loadingIndicadores.Contains(objetivoId))
            await LoadIndicadoresParaObjetivo(objetivoId);
    }

    private async Task LoadIndicadoresParaObjetivo(int objetivoId)
    {
        loadingIndicadores.Add(objetivoId);
        StateHasChanged();
        try
        {
            var result = await Mediator.Send(new GetAllIndicadoresByObjetivoRequest(objetivoId), _cts.Token);
            var indicadores = (result.IsSuccess && result.Value != null) ? result.Value.ToList() : new List<IndicadorDto>();
            
            indicadores = indicadores
                .Where(i => i.Proceso != null && i.Proceso.Id == ProcesoId)
                .OrderBy(i => i.Id)
                .ToList();

            indicadoresPorObjetivo[objetivoId] = indicadores;
        }
        catch (OperationCanceledException) { }
        catch
        {
            indicadoresPorObjetivo[objetivoId] = new List<IndicadorDto>();
        }
        finally
        {
            loadingIndicadores.Remove(objetivoId);
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}