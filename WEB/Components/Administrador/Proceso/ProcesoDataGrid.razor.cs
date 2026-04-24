using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Proceso.Dto;

namespace WEB.Components.Administrador.Proceso;

public partial class ProcesoDataGrid : ComponentBase
{
    [Parameter] public List<ProcesoDto> Procesos { get; set; } = new();
    [Parameter] public EventCallback<ProcesoDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ProcesoDto> OnDelete { get; set; }
    [Parameter] public EventCallback<ProcesoDto> OnViewDetails { get; set; }
    [Parameter] public EventCallback SelectionChanged { get; set; } 
    
    private string nameFilter = string.Empty;
    private string? tipoFilter = null;
    private PaginationState paginationState = new() { ItemsPerPage = 10 };

    protected override async Task OnInitializedAsync()
    {

    }
    
    private IEnumerable<ProcesoDto> FilteredProcesos =>
        Procesos.Where(a =>
            (string.IsNullOrEmpty(nameFilter) || a.Nombre.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
        );
    
    private async Task OnItemSelect(ProcesoDto item, bool selected)
    {
        item.IsSelected = selected;
        await SelectionChanged.InvokeAsync();
        StateHasChanged();
    }

    private async Task OnSelectAllChanged(bool? all)
    {
        Procesos.ForEach(a => a.IsSelected = (all == true));
        await SelectionChanged.InvokeAsync();
        StateHasChanged();
    }

    public IReadOnlyList<ProcesoDto> SelectedItems => Procesos.Where(a => a.IsSelected).ToList();
    public bool HasSelection => SelectedItems?.Any() ?? false;
}