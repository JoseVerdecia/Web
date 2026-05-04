using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Icons.Filled;
using WEB.Common;
using WEB.Components.Administrador.Indicador;
using WEB.Core.Mediator;
using WEB.Features.Indicador.Delete;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Indicador.Restore;
using WEB.Features.Proceso.Get;
using WEB.Interfaces;

namespace WEB.Components.JefeProceso;

public partial class JPPage : ComponentBase
{
    private CancellationTokenSource _cancellationToken = new();

    // Activos
    private List<IndicadorDto> indicadores = new();
    private List<IndicadorDisplayItem> gridItems = new();
    private bool isLoadingActivos = true;
    private IndicadorDisplayItem? selectedIndicador; 

    // Papelera
    private List<IndicadorDisplayItem> deletedGridItems = new();
    private bool isLoadingPapelera = false;
    private IndicadorDeletedDataGrid deletedDataGrid = default!;

    private string procesoNombre = "Cargando...";
    private string ActiveTabId = "tab-activos";
    private string? jefeProcesoId;

    protected override async Task OnInitializedAsync()
    {
        var user = await CurrentUser.GetUserAsync();
        jefeProcesoId = user?.Id;

        await LoadProcesoNombreAsync(_cancellationToken.Token);
        await LoadIndicadoresActivos();
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == "tab-papelera" && !deletedGridItems.Any() && !isLoadingPapelera)
        {
            await LoadIndicadoresEliminados();
        }
    }

    // <-- NUEVO: Manejar selección de fila
    private void HandleRowSelected(IndicadorDisplayItem item)
    {
        selectedIndicador = item;
    }

    private async Task LoadProcesoNombreAsync(CancellationToken cancellationToken)
    {
        var user = await CurrentUser.GetUserAsync(cancellationToken);
        if (user == null)
        {
            procesoNombre = "No asignado";
            return;
        }

        var result = await Mediator.Send(new GetProcesoByJefeIdRequest(user.Id), cancellationToken);
        procesoNombre = result.IsSuccess && result.Value != null
            ? result.Value.Nombre
            : "No asignado";
    }

    private async Task LoadIndicadoresActivos()
    {
        isLoadingActivos = true;
        StateHasChanged();
        try
        {
            var allIndicadores = new List<IndicadorDto>();
            var currentPage = 1;
            var pageSize = 50;
            var totalCount = 0;

            do
            {
                var result = await Mediator.Send(
                    new GetAllIndicadoresRequest(Page: currentPage, PageSize: pageSize),
                    _cancellationToken.Token);

                if (result.IsSuccess && result.Value?.Items != null)
                {
                    allIndicadores.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else
                {
                    break;
                }
            } while (allIndicadores.Count < totalCount);

            indicadores = allIndicadores;
            gridItems = indicadores.Select(IndicadorDisplayItem.FromIndicadorDto).ToList();
        }
        finally
        {
            isLoadingActivos = false;
            StateHasChanged();
        }
    }

    private async Task LoadIndicadoresEliminados()
    {
        if (string.IsNullOrEmpty(jefeProcesoId)) return;

        isLoadingPapelera = true;
        StateHasChanged();
        try
        {
            var allDeleted = new List<IndicadorDto>();
            var currentPage = 1;
            var pageSize = 50;
            var totalCount = 0;

            do
            {
                var result = await Mediator.Send(
                    new GetSoftDeletedIndicadoresByJefeProcesoRequest(currentPage, pageSize, jefeProcesoId),
                    _cancellationToken.Token);

                if (result.IsSuccess && result.Value?.Items != null)
                {
                    allDeleted.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else
                {
                    break;
                }
            } while (allDeleted.Count < totalCount);

            deletedGridItems = allDeleted.Select(IndicadorDisplayItem.FromIndicadorDto).ToList();
        }
        finally
        {
            isLoadingPapelera = false;
            StateHasChanged();
        }
    }

    private async Task OpenCreateWizardDialog()
    {
        var parameters = new DialogParameters<CreateIndicadorRequest>
        {
            ShowTitle = false,
            PreventDismissOnOverlayClick = true,
            PrimaryAction = null,
            SecondaryAction = null,
            Modal = true,
            Width = "850px",
            ShowDismiss = false,
        };

        var emptyRequest = new CreateIndicadorRequest
        {
            Nombre = "",
            MetaCumplir = "",
            MetaReal = null,
            Origen = "",
            Tipo = "",
            Observacion = null,
            ProcesoId = 0,
            ObjetivoIds = new List<int>(),
            MetaCumplirPorArea = null
        };

        var dialog = await DialogService.ShowDialogAsync<IndicadorWizardDialog>(emptyRequest, parameters);
        var result = await dialog.Result;

        if (!result.Cancelled && result.Data is IndicadorDto newIndicador)
        {
            indicadores.Add(newIndicador);
            gridItems = indicadores.Select(IndicadorDisplayItem.FromIndicadorDto).ToList();
            StateHasChanged();
        }
    }

    private void OpenEdit(IndicadorDisplayItem displayItem)
    {
        NavigationManager.NavigateTo($"/indicadores/jefeProceso/form/{displayItem.Id}");
    }

    private async Task DeleteIndicador(IndicadorDisplayItem displayItem)
    {
        var indicador = indicadores.FirstOrDefault(i => i.Id == displayItem.Id);
        if (indicador == null) return;

        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar el indicador '{indicador.Nombre}'?"),
                Icon = new Size24.Games(),
                IconColor = Color.Warning
            },
            PrimaryAction = "Borrar",
            SecondaryAction = "Cancelar",
            Width = "300px"
        });

        var dialogResult = await dialog.Result;

        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(new DeleteIndicadorRequest(indicador.Id, Permanent: false));
            if (result.IsSuccess)
            {
                indicadores.RemoveAll(i => i.Id == indicador.Id);
                gridItems = indicadores.Select(IndicadorDisplayItem.FromIndicadorDto).ToList();

                // <-- NUEVO: Cerrar panel si se elimina el indicador seleccionado
                if (selectedIndicador?.Id == indicador.Id)
                    selectedIndicador = null;

                StateHasChanged();
            }
        }
    }

    private async Task ViewIndicadorDetails(IndicadorDisplayItem displayItem)
    {
        var indicador = indicadores.FirstOrDefault(i => i.Id == displayItem.Id);
        if (indicador == null) return;

        var parameters = new DialogParameters<IndicadorDto>
        {
            Title = "Detalles del Indicador",
            PrimaryAction = null,
            SecondaryAction = null,
            PreventDismissOnOverlayClick = true,
            Modal = true
        };

        await DialogService.ShowDialogAsync<IndicadorDetailDialog>(indicador, parameters);
    }

    // --- Papelera ---

    private async Task RestoreSingleIndicador(IndicadorDisplayItem displayItem)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración",
                MarkupMessage = new MarkupString($"¿Restaurar el indicador '{displayItem.Nombre}'?"),
                Icon = new Size20.ArrowSync(),
                IconColor = Color.Success
            },
            PrimaryAction = "Restaurar",
            SecondaryAction = "Cancelar"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var request = new RestoreIndicadoresRequest(new List<int> { displayItem.Id });
            var result = await Mediator.Send(request);
            if (result.IsSuccess)
            {
                deletedGridItems.RemoveAll(i => i.Id == displayItem.Id);
                await LoadIndicadoresActivos();
                StateHasChanged();
            }
        }
    }

    private async Task DeletePermanently(IndicadorDisplayItem displayItem)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminar permanentemente",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente '{displayItem.Nombre}'? Esta acción no se puede deshacer."),
                Icon = new Size20.Delete(),
                IconColor = Color.Error
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(new DeleteIndicadorRequest(displayItem.Id, Permanent: true));
            if (result.IsSuccess)
            {
                deletedGridItems.RemoveAll(i => i.Id == displayItem.Id);
                StateHasChanged();
            }
        }
    }

    public void Dispose()
    {
        _cancellationToken.Cancel();
        _cancellationToken.Dispose();
    }
}