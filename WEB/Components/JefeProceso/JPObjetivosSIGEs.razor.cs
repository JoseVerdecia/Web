    using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.Get;

namespace WEB.Components.JefeProceso;

public partial class JPObjetivosSIGEs : ComponentBase
{
   private ProcesoDto? procesoActual;
private List<ObjetivoDto> objetivos = new();
private bool isLoading = true;
private string activeTabId = "";
private bool isExporting = false;
private CancellationTokenSource _cts = new();

protected override async Task OnInitializedAsync()
{
    await LoadData();
}

private async Task LoadData()
{
    isLoading = true;
    try
    {
        string? jefeId = await CurrentUser.GetUserIdAsync();
        if (string.IsNullOrEmpty(jefeId)) return;

        var procesoResult = await Mediator.Send(new GetProcesoByJefeIdRequest(jefeId));
        if (procesoResult.IsSuccess && procesoResult.Value != null)
        {
            procesoActual = procesoResult.Value;

            var objetivosResult = await Mediator.Send(new GetAllObjetivosByProcesoRequest(procesoActual.Id));
            if (objetivosResult.IsSuccess)
                objetivos = objetivosResult.Value;

            if (objetivos.Any())
                activeTabId = $"tab-{objetivos.First().Id}";
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error cargando datos: {ex.Message}");
    }
    finally
    {
        isLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}

private void HandleTabChange(FluentTab tab)
{
    if (tab.Id != null) activeTabId = tab.Id;
}

private int? GetCurrentObjectiveId()
{
    var idStr = activeTabId?.Replace("tab-", "");
    return int.TryParse(idStr, out int id) ? id : null;
}

private async Task ExportCurrentObjectiveToExcel()
{
    if (procesoActual == null) return;
    var objId = GetCurrentObjectiveId();
    if (objId == null) return;
    isExporting = true;
    try
    {
        var data = await ExcelExportService.ExportJPObjetivoSIGEToExcelAsync(objId.Value, procesoActual.Id);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{objId}.xlsx", Convert.ToBase64String(data));
    }
    finally { isExporting = false; }
}
private async Task ExportAllObjectiveToExcel()
{
    
    isExporting = true;
    try
    {
        var data = await JefeProcesoExportServices.ExportAllObjetivosProcesoSIGEToExcelAsync(procesoActual.Id);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{procesoActual.Id}.xlsx", Convert.ToBase64String(data));
    }
    finally { isExporting = false; }
}

private async Task ExportCurrentObjectiveToPdf()
{
    if (procesoActual == null) return;
    var objId = GetCurrentObjectiveId();
    if (objId == null) return;
    isExporting = true;
    try
    {
        var data = await ExportPdfService.ExportObjetivoProcesoSIGEToPdfAsync(objId.Value, procesoActual.Id);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{objId}.pdf", Convert.ToBase64String(data));
    }
    finally { isExporting = false; }
}

private async Task ExportAllObjectiveToPdf()
{
  
    isExporting = true;
    try
    {
        var data = await JefeProcesoExportServices.ExportAllObjetivosProcesoSIGEToPdfAsync(procesoActual.Id);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{procesoActual.Id}.pdf", Convert.ToBase64String(data));
    }
    finally { isExporting = false; }
}

public void Dispose()
{
    _cts.Cancel();
    _cts.Dispose();
}
}