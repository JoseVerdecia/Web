using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Objetivo.Dto;

namespace WEB.Components.Administrador.Objetivo;

public partial class ObjetivoDataGrid : ComponentBase
{
    [Parameter] public List<ObjetivoDto> objetivos { get; set; } = new();
    [Parameter] public EventCallback<ObjetivoDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ObjetivoDto> OnDelete { get; set; }
    [Parameter] public EventCallback<ObjetivoDto> OnViewDetails { get; set; }
    [Parameter] public EventCallback SelectionChanged { get; set; }

    private string nameFilter = string.Empty;
    private PaginationState paginationState = new() { ItemsPerPage = 10 };

    protected override async Task OnInitializedAsync()
    {

    }

    private IEnumerable<ObjetivoDto> FilteredObjetivos =>
        objetivos.Where(a =>
            (string.IsNullOrEmpty(nameFilter) || a.Nombre.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
        );
    
    private async Task OnItemSelect(ObjetivoDto item, bool selected)
    {
        item.IsSelected = selected;
        await SelectionChanged.InvokeAsync();
        StateHasChanged();
    }

    private async Task OnSelectAllChanged(bool? all)
    {
        objetivos.ForEach(a => a.IsSelected = (all == true));
        await SelectionChanged.InvokeAsync();
        StateHasChanged();
    }

    public IReadOnlyList<ObjetivoDto> SelectedItems => objetivos.Where(a => a.IsSelected).ToList();
    public bool HasSelection => SelectedItems?.Any() ?? false;
}