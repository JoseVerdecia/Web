using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;

namespace WEB.Components.Shared.Pages;

public partial class ObjetivosSIGEs : ComponentBase
{
    private List<ObjetivoDto> objetivos = new();
    private bool isLoadingObjetivos = true;
    private string activeTabId = "";
    private string currentObjetivoNombre = string.Empty;
    private bool isExporting = false;
    private int? currentObjetivoId => objetivos.FirstOrDefault(o => o.Id.ToString() == activeTabId?.Replace("tab-", ""))?.Id;   
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadObjetivos();
        }
    }

  

    private async Task LoadObjetivos()
    {
        isLoadingObjetivos = true;
        try
        {
            var allObjetivos = new List<ObjetivoDto>();
            int page = 1;
            int pageSize = 50;
            int totalCount;
            do
            {
                var result = await Mediator.Send(new GetAllObjetivosRequest(page, pageSize));
                if (result?.Value?.Items != null)
                {
                    allObjetivos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    page++;
                }
                else break;
            } while (allObjetivos.Count < totalCount);

            objetivos = allObjetivos.OrderBy(o => o.NumeroObjetivo).ToList();
            if (objetivos.Any())
            {
                activeTabId = $"tab-{objetivos.First().Id}";
                currentObjetivoNombre = objetivos.First().Nombre;
            }
        }
        finally
        {
            isLoadingObjetivos = false;
            StateHasChanged();
        }
    }
    
    private async Task ExportCurrentObjectiveToPdf()
    {
        if (!currentObjetivoId.HasValue) return;
        isExporting = true;
        try
        {
            byte[] pdfBytes = await ExportPdfService.ExportObjetivoSIGEToPdfAsync(currentObjetivoId.Value);
            string fileName = $"Objetivo_{currentObjetivoNombre?.Replace(" ", "_")}.pdf";
            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, Convert.ToBase64String(pdfBytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar PDF: {ex.Message}");
        }
        finally
        {
            isExporting = false;
        }
    }
    
    private async Task ExportCurrentObjectiveToExcel()
    {
        if (!currentObjetivoId.HasValue) return;
        isExporting = true;
        try
        {
            byte[] excelBytes = await ExcelExportService.ExportObjetivoSIGEToExcelAsync(currentObjetivoId.Value);
            string fileName = $"Objetivo_{currentObjetivoNombre?.Replace(" ", "_")}.xlsx";
            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, Convert.ToBase64String(excelBytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exportando Excel: {ex.Message}");
        }
        finally
        {
            isExporting = false;
        }
    }

    private void HandleTabChange(FluentTab tab)
    {
        if (tab.Id == null) return;
        var idStr = tab.Id.Replace("tab-", "");
        if (int.TryParse(idStr, out int objetivoId))
        {
            var objetivoActivo = objetivos.FirstOrDefault(o => o.Id == objetivoId);
            currentObjetivoNombre = objetivoActivo?.Nombre ?? string.Empty;
            StateHasChanged();
        }
    }
}