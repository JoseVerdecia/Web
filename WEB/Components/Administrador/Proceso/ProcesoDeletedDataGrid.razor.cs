using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Proceso.Dto;

namespace WEB.Components.Administrador.Proceso;

public partial class ProcesoDeletedDataGrid : ComponentBase
{
    [Parameter] public List<ProcesoDto> Procesos { get; set; } = new();
    [Parameter] public HashSet<int> SelectedIds { get; set; } = new();
    [Parameter] public EventCallback<HashSet<int>> SelectedIdsChanged { get; set; }
    [Parameter] public EventCallback<ProcesoDto> OnRestore { get; set; }
    [Parameter] public EventCallback<ProcesoDto> OnDeletePermanently { get; set; }

    private string nameFilter = string.Empty;
    private PaginationState paginationState = new() { ItemsPerPage = 10 };

    private IEnumerable<ProcesoDto> SelectedProcesos
    {
        get => Procesos.Where(p => SelectedIds.Contains(p.Id));
        set
        {
            SelectedIds = value.Select(p => p.Id).ToHashSet();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }

    private bool SelectAllCheckbox
    {
        get => SelectedIds.Count > 0 && SelectedIds.Count == FilteredProcesos.Count();
        set
        {
            if (value)
                SelectedIds = FilteredProcesos.Select(p => p.Id).ToHashSet();
            else
                SelectedIds.Clear();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }

    private IEnumerable<ProcesoDto> FilteredProcesos =>
        Procesos.Where(p =>
            string.IsNullOrEmpty(nameFilter) || p.Nombre.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)
        );
}