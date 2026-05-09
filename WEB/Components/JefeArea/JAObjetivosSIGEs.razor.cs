using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using WEB.Core.Mediator;
using WEB.Core.Services;
using WEB.Features.Area.Get;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;
using WEB.Services;

namespace WEB.Components.JefeArea;

public partial class JAObjetivosSIGEs : ComponentBase
{
  
    private List<ObjetivoDto> objetivos = new();
    private int? areaId;
    private string nombreArea = "";
    private bool isLoading = true;
    private string activeTabId = "";
    private string currentObjetivoNombre = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            try
            {
        
                var user = await CurrentUser.GetUserAsync();
                var areaResult = await Mediator.Send(new GetAreaByJefeIdRequest(user.Id));
                if (areaResult.IsSuccess && areaResult.Value != null)
                {
                    areaId = areaResult.Value.Id;
                    nombreArea = areaResult.Value.Nombre;
                }
                    
                var allObjetivos = new List<ObjetivoDto>();
                int page = 1, pageSize = 50, totalCount;
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
                isLoading = false;
                StateHasChanged();
            }    
        }
    }

    

    private Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == null) return Task.CompletedTask;
        var idStr = tab.Id.Replace("tab-", "");
        if (int.TryParse(idStr, out int objetivoId))
        {
            var objetivoActivo = objetivos.FirstOrDefault(o => o.Id == objetivoId);
            currentObjetivoNombre = objetivoActivo?.Nombre ?? string.Empty;
            StateHasChanged();
        }
        return Task.CompletedTask;
    }

    private int? GetCurrentObjectiveId()
    {
        var idStr = activeTabId?.Replace("tab-", "");
        return int.TryParse(idStr, out int id) ? id : null;
    }

    private async Task ExportCurrentObjectiveToExcel()
    {
        if (areaId == null) return;
        var objId = GetCurrentObjectiveId();
        if (objId == null) return;
        var data = await ExcelExportService.ExportJAObjetivoSIGEToExcelAsync(objId.Value, areaId.Value);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{objId}_Area.xlsx", Convert.ToBase64String(data));
    }
    private async Task ExportAllObjectiveToExcel()
    {
        var data = await JefeAreaExportServices.ExportJAAllObjetivosSIGEToExcelAsync(areaId.Value);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{areaId}_Area.xlsx", Convert.ToBase64String(data));
    }

    private async Task ExportCurrentObjectiveToPdf()
    {
        if (areaId == null) return;
        var objId = GetCurrentObjectiveId();
        if (objId == null) return;
        var data = await ExportPdfService.ExportJAObjetivoSIGEToPdfAsync(objId.Value, areaId.Value);
        await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{objId}_Area.pdf", Convert.ToBase64String(data));
    }
    private async Task ExportAllObjectiveToPdf()
        {
            var data = await JefeAreaExportServices.ExportJAAllObjetivosSIGEToPdfAsync(areaId.Value);
            await JS.InvokeVoidAsync("downloadFile", $"Objetivo_{areaId}_Area.pdf", Convert.ToBase64String(data));
        }
}