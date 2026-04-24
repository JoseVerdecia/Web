using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Helpers;
using WEB.Enums;

namespace WEB.Components.Administrador.Indicador;

public partial class IndicadorDeletedDataGrid : ComponentBase
{
    [Parameter] public List<IndicadorDisplayItem> Items { get; set; } = new();
    /*[Parameter] public HashSet<int> SelectedIds { get; set; } = new();*/
    /*[Parameter] public EventCallback<HashSet<int>> SelectedIdsChanged { get; set; }*/
    [Parameter] public EventCallback<IndicadorDisplayItem> OnRestore { get; set; }
    [Parameter] public EventCallback<IndicadorDisplayItem> OnDeletePermanently { get; set; }
    [Parameter] public EventCallback SelectionChanged { get; set; }
    public IReadOnlyList<IndicadorDisplayItem> SelectedItems => Items.Where(i => i.IsSelected).ToList();
    public bool HasSelection => SelectedItems.Any();

    private string nameFilter = string.Empty;
    private string? tipoFilter;
    private string? origenFilter;
    private string? evaluacionFilter;
    private string? procesoFilter;

    private List<SelectOption<string?>> tipoOptions = new();
    private List<SelectOption<string?>> origenOptions = new();
    private List<SelectOption<string?>> evaluacionOptions = new();
    private List<SelectOption<string?>> procesoOptions = new();

    private PaginationState paginationState = new() { ItemsPerPage = 10 };

    /*private IEnumerable<IndicadorDisplayItem> SelectedIndicadores
    {
        get => Items.Where(i => SelectedIds.Contains(i.Id));
        set
        {
            SelectedIds = value.Select(i => i.Id).ToHashSet();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }*/

    /*private bool SelectAllCheckbox
    {
        get => SelectedIds.Count == FilteredItems.Count();
        set
        {
            if (value)
                SelectedIds = FilteredItems.Select(i => i.Id).ToHashSet();
            else
                SelectedIds.Clear();
            SelectedIdsChanged.InvokeAsync(SelectedIds);
        }
    }*//**/

    protected override void OnInitialized()
    {
        tipoOptions = EnumHelper.GetOptions<IndicadorTipo>(includeAllOption: false);
        origenOptions = EnumHelper.GetOptions<IndicadorOrigen>(includeAllOption: false);
    }

    protected override void OnParametersSet()
    {
        LoadDynamicOptions();
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
            (string.IsNullOrEmpty(tipoFilter) || i.Tipo == tipoFilter) &&
            (string.IsNullOrEmpty(origenFilter) || i.Origen == origenFilter) &&
            (string.IsNullOrEmpty(evaluacionFilter) || i.Evaluacion.GetDisplayName() == evaluacionFilter) &&
            (string.IsNullOrEmpty(procesoFilter) || i.ProcesoNombre == procesoFilter)
        );

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

    private async Task ResetPagination()
    {
        await paginationState.SetCurrentPageIndexAsync(0);
    }
}