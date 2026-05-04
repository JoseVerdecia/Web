using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Common;
using WEB.Core.Result;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;
using WEB.Services;

namespace WEB.Components.Shared.Pages;

public partial class ProcesosSIGES : ComponentBase
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
            var currentPage = 1;
            var totalCount = 0;
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

    private void HandleTabChange(FluentTab tab)
    {
        if (tab.Id != null)
            activeTabId = tab.Id;
    }

    private async Task ExportToExcel()
    {
        if (string.IsNullOrEmpty(activeTabId)) return;
        var idStr = activeTabId.Replace("tab-", "");
        if (!int.TryParse(idStr, out int procesoId)) return;

        var data = await ExcelExportService.ExportProcesoToExcelSIEGEAsync(procesoId);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"Proceso_{procesoId}.xlsx", Convert.ToBase64String(data));
    }

    private async Task ExportToPdf()
    {
        if (string.IsNullOrEmpty(activeTabId)) return;
        var idStr = activeTabId.Replace("tab-", "");
        if (!int.TryParse(idStr, out int procesoId)) return;

        var data = await ExcelExportService.ExportProcesoToPdfSIEGEAsync(procesoId);
        await JSRuntime.InvokeVoidAsync("downloadFile", $"Proceso_{procesoId}.pdf", Convert.ToBase64String(data));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}