using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.Shared.Pages;

public partial class ProyectoEstrategicoProcesos : ComponentBase
{
    
       private CancellationTokenSource _cts = new();
    private List<ProcesoDto> procesos = new();
    private bool isLoadingProcesos = true;
    private string activeTabId = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadProcesos();
    }

    private async Task LoadProcesos()
    {
        isLoadingProcesos = true;
        StateHasChanged();
        try
        {
            const int pageSize = 50;
            int currentPage = 1;
            int totalCount = 0;
            var allProcesos = new List<ProcesoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllProcesosRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value?.Items != null)
                {
                    allProcesos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allProcesos.Count < totalCount && !_cts.IsCancellationRequested);

            procesos = allProcesos;

         
            if (procesos.Any())
                activeTabId = $"tab-{procesos.First().Id}";
        }
        catch (OperationCanceledException) { }
        catch { }
        finally
        {
            isLoadingProcesos = false;
            await InvokeAsync(StateHasChanged);
        }
    }

   
    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id != null)
            activeTabId = tab.Id;
        await Task.CompletedTask;
    }

    private async Task ExportExcel()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        if (int.TryParse(idStr, out int procesoId))
        {
            var excelData = await ExportService.ExportProcesoTabToExcelAsync(procesoId);
            await JSRuntime.InvokeVoidAsync("downloadFile", "Proceso.xlsx", Convert.ToBase64String(excelData));
        }
    }

    private async Task ExportPdf()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        if (int.TryParse(idStr, out int procesoId))
        {
            var pdfData = await ExportPdfService.ExportProcesoTabToPdfAsync(procesoId); 
            await JSRuntime.InvokeVoidAsync("downloadFile", "Proceso.pdf", Convert.ToBase64String(pdfData));
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}