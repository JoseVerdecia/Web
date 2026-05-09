using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Features.Area.Get;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.JefeArea;

public partial class JAProcesosSIGES : ComponentBase
{
      private List<ProcesoDto> procesos = new();
    private int? areaId;
    private bool isLoading = true;
    private string activeTabId = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = await CurrentUser.GetUserAsync();
            var areaResult = await Mediator.Send(new GetAreaByJefeIdRequest(user.Id));
            if (areaResult.IsSuccess && areaResult.Value != null)
                areaId = areaResult.Value.Id;

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
        }
    }

    private void HandleTabChange(FluentTab tab)
    {
        if (tab.Id != null)
            activeTabId = tab.Id;
    }

    private async Task ExportToExcel()
    {
        if (string.IsNullOrEmpty(activeTabId) || areaId == null) return;
        var idStr = activeTabId.Replace("tab-", "");
        if (!int.TryParse(idStr, out int procesoId)) return;

        var data = await ExcelExportService.ExportJAProcesoSIGEToExcelAsync(procesoId, areaId.Value);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"Proceso_{procesoId}_Area.xlsx", Convert.ToBase64String(data));
    }

    private async Task ExportToPdf()
    {
        if (string.IsNullOrEmpty(activeTabId) || areaId == null) return;
        var idStr = activeTabId.Replace("tab-", "");
        if (!int.TryParse(idStr, out int procesoId)) return;

        var data = await ExportPdf.ExportJAProcesoSIGEToPdfAsync(procesoId, areaId.Value);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"Proceso_{procesoId}_Area.pdf", Convert.ToBase64String(data));
    }
}