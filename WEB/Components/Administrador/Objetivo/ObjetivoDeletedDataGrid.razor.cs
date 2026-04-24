using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Objetivo.Dto;

namespace WEB.Components.Administrador.Objetivo;

public partial class ObjetivoDeletedDataGrid : ComponentBase
{
    [Parameter] public List<ObjetivoDto> Objetivos { get; set; } = new();
    [Parameter] public HashSet<int> SelectedIds { get; set; } = new();
    [Parameter] public EventCallback<HashSet<int>> SelectedIdsChanged { get; set; }
    [Parameter] public EventCallback<ObjetivoDto> OnRestore { get; set; }
    [Parameter] public EventCallback<ObjetivoDto> OnDeletePermanently { get; set; }

    private string nameFilter = string.Empty;
    private PaginationState paginationState = new() { ItemsPerPage = 10 };

    private IEnumerable<ObjetivoDto> SelectedObjetivos
    {
        get => Objetivos.Where(o => SelectedIds.Contains(o.Id));
        set
        {
            SelectedIds = value.Select(o => o.Id).ToHashSet();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }

    private bool SelectAllCheckbox
    {
        get => SelectedIds.Count == FilteredObjetivos.Count();
        set
        {
            if (value)
                SelectedIds = FilteredObjetivos.Select(o => o.Id).ToHashSet();
            else
                SelectedIds.Clear();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }

    private IEnumerable<ObjetivoDto> FilteredObjetivos =>
        Objetivos.Where(o =>
            string.IsNullOrEmpty(nameFilter) ||
            o.Nombre.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)
        );
}