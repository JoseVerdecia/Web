using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Helpers;
using WEB.Enums;
using WEB.Features.Area.Dto;

namespace WEB.Components.Administrador.Area;

public partial class AreaDataGrid : ComponentBase
{
    [Parameter] public List<AreaDto> Areas { get; set; } = new();
    [Parameter] public EventCallback<AreaDto> OnEdit { get; set; }
    [Parameter] public EventCallback<AreaDto> OnDelete { get; set; }
    [Parameter] public EventCallback<AreaDto> OnViewDetails { get; set; }
    

    private string nameFilter = string.Empty;
    private string? tipoFilter;  
    private List<SelectOption<string?>> tipoOptions = new();
    private PaginationState paginationState = new() { ItemsPerPage = 10 };
    [Parameter] public EventCallback SelectionChanged { get; set; }
    
    private async Task OnItemSelect(AreaDto item, bool selected)
    {
        item.IsSelected = selected;
        await SelectionChanged.InvokeAsync();
        StateHasChanged();
    }

    private async Task OnSelectAllChanged(bool? all)
    {
        Areas.ForEach(a => a.IsSelected = (all == true));
        await SelectionChanged.InvokeAsync();
        StateHasChanged();
    }

    public IReadOnlyList<AreaDto> SelectedItems => Areas.Where(a => a.IsSelected).ToList();
    public bool HasSelection => SelectedItems?.Any() ?? false;
    
    protected override async Task OnInitializedAsync()
    {
        tipoOptions = EnumHelper.GetOptions<AreaTipo>(includeAllOption:false);

    }

    private void ClearTipoFilter()
    {
        tipoFilter = null;  
        StateHasChanged();
    }

    private IEnumerable<AreaDto> FilteredAreas =>
        Areas.Where(a =>
            (string.IsNullOrEmpty(nameFilter) || a.Nombre.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(tipoFilter) || a.Tipo.ToString() == tipoFilter)
        );

}