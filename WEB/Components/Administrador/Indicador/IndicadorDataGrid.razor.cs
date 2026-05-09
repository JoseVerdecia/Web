using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Helpers;
using WEB.Enums;

namespace WEB.Components.Administrador.Indicador;

public partial class IndicadorDataGrid : ComponentBase
{
    [Parameter] public string AreasBaseUrl { get; set; } = "/indicadores";
    [Parameter] public string EditBaseUrl { get; set; } = "/indicadores/form";
    [Parameter] public List<IndicadorDisplayItem> Items { get; set; } = new();
    [Parameter] public EventCallback<IndicadorDisplayItem> OnEdit { get; set; }
    [Parameter] public EventCallback<IndicadorDisplayItem> OnDelete { get; set; }
    [Parameter] public EventCallback<IndicadorDisplayItem> OnViewDetails { get; set; }
    [Parameter] public EventCallback<IndicadorDisplayItem> OnRowSelected { get; set; }
    [Parameter] public EventCallback SelectionChanged { get; set; }
 
    private async Task OnItemSelect(IndicadorDisplayItem item, bool selected)
    {
        item.IsSelected = selected;
        await SelectionChanged.InvokeAsync();
    }
    
    private void NavigateToHistorial(int id) => NavigationManager.NavigateTo($"/notificaciones/indicador/{id}");
    
    private async Task OnSelectAllChanged(bool? all)
    {
        Items.ForEach(i => i.IsSelected = (all == true));
        await SelectionChanged.InvokeAsync();
    }
    
    public IReadOnlyList<IndicadorDisplayItem> SelectedItems => Items.Where(i => i.IsSelected).ToList();
    public bool HasSelection => SelectedItems?.Any() ?? false;
    
    private string nameFilter = string.Empty;
    private PaginationState paginationState = new() { ItemsPerPage = 10 };
    private int? _selectedItemId;
    
    private string? tipoFilter;
    private string? origenFilter;
    private string? evaluacionFilter;
    private string? procesoFilter;
    
    private List<SelectOption<string?>> tipoOptions = new();
    private List<SelectOption<string?>> origenOptions = new();
    private List<SelectOption<string?>> evaluacionOptions = new();
    private List<SelectOption<string?>> procesoOptions = new();
    
    private void NavigateToAreas(int id) => NavigationManager.NavigateTo($"{AreasBaseUrl}/{id}/areas");
    private void NavigateToEdit(int id) => NavigationManager.NavigateTo($"{EditBaseUrl}/{id}");
    
    protected override void OnInitialized()
    {
        
        tipoOptions = EnumHelper.GetOptions<IndicadorTipo>(includeAllOption: false);
        origenOptions = EnumHelper.GetOptions<IndicadorOrigen>(includeAllOption: false);
    }
    private async Task ClearAllFilters()
    {
        nameFilter = string.Empty;
        evaluacionFilter = string.Empty;
        origenFilter = string.Empty;
        tipoFilter = string.Empty;
        procesoFilter = string.Empty;
    
        await ResetPagination();
    }
    private List<IndicadorDisplayItem> _previousItems = new();
    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(Items, _previousItems))
        {
            _previousItems = Items;
            LoadDynamicOptions();
        }
    }
    
    private void LoadDynamicOptions()
    {
        
        evaluacionOptions = Items
            .Select(i => i.Evaluacion!)
            .Distinct()
            .OrderBy(e => e)
            .Select(e => new SelectOption<string?> 
            { 
                Value = e.GetDisplayName(), 
                Text = e.GetDisplayName() 
            })
            .ToList();

        procesoOptions = Items
            .Where(i => !string.IsNullOrEmpty(i.ProcesoNombre))
            .Select(i => i.ProcesoNombre!)
            .Distinct()
            .OrderBy(p => p)
            .Select(p => new SelectOption<string?> 
            { 
                Value = p, 
                Text = p ?? string.Empty 
            })
            .ToList();
    }

    private IEnumerable<IndicadorDisplayItem> FilteredItems =>
        Items.Where(i =>
        
            (string.IsNullOrEmpty(nameFilter) || 
             i.Nombre.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) &&
         
            (string.IsNullOrEmpty(tipoFilter) || 
             i.Tipo == tipoFilter) &&
            
            (string.IsNullOrEmpty(origenFilter) || 
             i.Origen == origenFilter) &&
            
            (string.IsNullOrEmpty(evaluacionFilter) || 
             i.Evaluacion.GetDisplayName() == evaluacionFilter) &&
           
            (string.IsNullOrEmpty(procesoFilter) || 
             i.ProcesoNombre == procesoFilter)
        );
    
        
    
    
    private string GetRowClass(IndicadorDisplayItem item)
    {
        return item.Id == _selectedItemId ? "selected-row" : "";
    }
    
  
    private string FormatObjetivos(string? objetivosFormatted)
    {
        if (string.IsNullOrEmpty(objetivosFormatted) || objetivosFormatted == "Sin objetivos")
            return objetivosFormatted ?? "Sin objetivos";

        return objetivosFormatted;
    }
    
    #region Limpiar Filtros

    private async Task ClearTipoFilter()
    {
        tipoFilter = null;
        await ResetPagination();
    }

    private async Task ClearOrigenFilter()
    {
        origenFilter = null;
        await ResetPagination();
    }

    private async Task ClearEvaluacionFilter()
    {
        evaluacionFilter = null;
        await ResetPagination();
    }

    private async Task ClearProcesoFilter()
    {
        procesoFilter = null;
        await ResetPagination();
    }

    #endregion
    
    #region Reset Paginación

    private async Task ResetPagination()
    {
        await paginationState.SetCurrentPageIndexAsync(0);
    }

    #endregion
    
}