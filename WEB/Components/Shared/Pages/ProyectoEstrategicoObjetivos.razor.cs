using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;

namespace WEB.Components.Shared.Pages;

public partial class ProyectoEstrategicoObjetivos : ComponentBase
{
    
   private CancellationTokenSource _cts = new();
    private List<ObjetivoDto> objetivos = new();
    private Dictionary<int, List<IndicadorDto>> indicadoresPorObjetivo = new();
    private HashSet<int> loadingIndicadores = new();
    private bool isLoadingObjetivos = true;
    private string activeTabId = "";
    private string currentObjetivoNombre = string.Empty;
   


    protected override async Task OnInitializedAsync()
    {
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
                activeTabId = $"tab-{objetivos.First().Id}";
                currentObjetivoNombre = objetivos.First().Nombre ?? string.Empty;
                await LoadIndicadores(objetivos.First().Id);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        finally
        {
            isLoadingObjetivos = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == null) return;
        var idStr = tab.Id.Replace("tab-", "");
        if (!int.TryParse(idStr, out int objetivoId)) return;
        
        var objetivoActivo = objetivos.FirstOrDefault(o => o.Id == objetivoId);
        currentObjetivoNombre = objetivoActivo?.Nombre ?? string.Empty;

        if (!indicadoresPorObjetivo.ContainsKey(objetivoId) && !loadingIndicadores.Contains(objetivoId))
            await LoadIndicadores(objetivoId);
        
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadIndicadores(int objetivoId)
    {
        loadingIndicadores.Add(objetivoId);
        StateHasChanged();
        try
        {
            var result = await Mediator.Send(new GetAllIndicadoresByObjetivoRequest(objetivoId), _cts.Token);
            indicadoresPorObjetivo[objetivoId] = (result.IsSuccess && result.Value != null)
                ? result.Value.ToList()
                : new List<IndicadorDto>();
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            indicadoresPorObjetivo[objetivoId] = new List<IndicadorDto>();
        }
        finally
        {
            loadingIndicadores.Remove(objetivoId);
            await InvokeAsync(StateHasChanged);
        }
    }
    private async Task ExportToExcel()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        if (int.TryParse(idStr, out int objetivoId))
        {
            var excelData = await ExportService.ExportObjetivoToExcelAsync(objetivoId);
            await JSRuntime.InvokeVoidAsync("downloadFile", "Objetivo.xlsx", Convert.ToBase64String(excelData));
        }

     
    }
    
    private async Task ExportToPdf()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        if (int.TryParse(idStr, out int objetivoId))
        {
            var pdfData = await ExportService.ExportObjetivoToPdfAsync(objetivoId);
            await JSRuntime.InvokeVoidAsync("downloadFile", "Objetivo.pdf", Convert.ToBase64String(pdfData));
        }
        
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    
    
}