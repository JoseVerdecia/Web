using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.Shared.Pages;

public partial class AdminEvaluacionProcesoPage : ComponentBase
{
    private List<ProcesoDto> procesos = new();
    private bool isLoading = true;
    private string activeTabId = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadProcesos();
    }

    private async Task LoadProcesos()
    {
        isLoading = true;
        try
        {
            const int pageSize = 50;
            int currentPage = 1;
            int totalCount = 0;
            var allProcesos = new List<ProcesoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllProcesosRequest(currentPage, pageSize));
                if (result?.Value?.Items != null)
                {
                    allProcesos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allProcesos.Count < totalCount);

            procesos = allProcesos;
            if (procesos.Any())
                activeTabId = $"tab-{procesos.First().Id}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    
 
    private async Task ExportExcel()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        if (int.TryParse(idStr, out int procesoId))
        {
            var excelData = await ExportService.ExportEvaluacionProcesoToExcelAsync(procesoId);
            await JSRuntime.InvokeVoidAsync("downloadFile", $"Evaluacion_{procesoId}.xlsx", Convert.ToBase64String(excelData));
        }
    }

    private async Task ExportPdf()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        if (int.TryParse(idStr, out int procesoId))
        {
            var pdfData = await ExportService.ExportEvaluacionProcesoToPdfAsync(procesoId);
            await JSRuntime.InvokeVoidAsync("downloadFile", $"Evaluacion_{procesoId}.pdf", Convert.ToBase64String(pdfData));
        }
    }
}