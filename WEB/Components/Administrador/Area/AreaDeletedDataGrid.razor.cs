using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Helpers;
using WEB.Enums;
using WEB.Features.Area.Dto;

namespace WEB.Components.Administrador.Area;

public partial class AreaDeletedDataGrid : ComponentBase
{
    [Parameter] public List<AreaDto> Areas { get; set; } = new();
    [Parameter] public HashSet<int> SelectedIds { get; set; } = new();
    [Parameter] public EventCallback<HashSet<int>> SelectedIdsChanged { get; set; }
    [Parameter] public EventCallback<AreaDto> OnRestore { get; set; }
    [Parameter] public EventCallback<AreaDto> OnDeletePermanently { get; set; }

    private string nameFilter = string.Empty;
    private string? tipoFilter;
    private List<SelectOption<string?>> tipoOptions = new();
    private PaginationState paginationState = new() { ItemsPerPage = 10 };

    private IEnumerable<AreaDto> SelectedAreas
    {
        get => Areas.Where(a => SelectedIds.Contains(a.Id));
        set
        {
            SelectedIds = value.Select(a => a.Id).ToHashSet();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }

    private bool SelectAllCheckbox
    {
        get => SelectedIds.Count == FilteredAreas.Count();
        set
        {
            if (value)
                SelectedIds = FilteredAreas.Select(a => a.Id).ToHashSet();
            else
                SelectedIds.Clear();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }

    protected override void OnInitialized()
    {
        tipoOptions = EnumHelper.GetOptions<AreaTipo>(includeAllOption: false);
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