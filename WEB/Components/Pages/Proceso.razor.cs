using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Icons.Filled;
using Microsoft.JSInterop;
using WEB.Components.Administrador.Proceso;
using WEB.Core.Mediator;
using WEB.Enums;
using WEB.Features.Proceso.Delete;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;
using WEB.Features.Proceso.Restore;

namespace WEB.Components.Pages;

public partial class Proceso : ComponentBase
{
    private CancellationTokenSource _cts = new();

    private List<ProcesoDto> activeProcesos = new();
    private List<ProcesoDto> deletedProcesos = new();
    private HashSet<int> selectedDeletedProcesoIds = new();
    private ProcesoDataGrid procesoDataGrid = default!;

    private bool isLoadingActivos = true;
    private bool isLoadingPapelera = false;

    private string ActiveTabId = "tab-activos";

    protected override async Task OnInitializedAsync()
    {
        await LoadActiveProcesos();
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == "tab-papelera" && !deletedProcesos.Any() && !isLoadingPapelera)
            await LoadDeletedProcesos();
    }

    private async Task OnGridSelectionChanged()
    {
        StateHasChanged();
        await Task.CompletedTask;
    }

    #region Carga de datos

    private async Task LoadActiveProcesos()
    {
        isLoadingActivos = true;
        StateHasChanged();
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allProcesos = new List<ProcesoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllProcesosRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value?.Items != null)
                {
                    allProcesos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allProcesos.Count < totalCount && !_cts.IsCancellationRequested);

            activeProcesos = allProcesos;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync("Error", $"No se pudieron cargar los procesos: {ex.Message}");
        }
        finally
        {
            isLoadingActivos = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadDeletedProcesos()
    {
        isLoadingPapelera = true;
        StateHasChanged();
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allProcesos = new List<ProcesoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllProcesosSoftDeletedRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value?.Items != null)
                {
                    allProcesos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allProcesos.Count < totalCount && !_cts.IsCancellationRequested);

            deletedProcesos = allProcesos;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync("Error", $"No se pudieron cargar los procesos eliminados: {ex.Message}");
        }
        finally
        {
            isLoadingPapelera = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Operaciones con procesos activos

    private async Task OpenCreateDialog()
    {
        var parameters = new DialogParameters<CreateProcesoRequest>
        {
            ShowTitle = false,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(95vw, 300px)",
            ShowDismiss = false,
        };

        var dialog = await DialogService.ShowDialogAsync<ProcesoFormDialog>(new CreateProcesoRequest(), parameters);
        var result = await dialog.Result;
        if (!result.Cancelled && result.Data is ProcesoDto newProceso)
        {
            activeProcesos = activeProcesos.Append(newProceso).ToList();
            StateHasChanged();
        }
    }

    private async Task OpenEditDialog(ProcesoDto proceso)
    {
        var parameters = new DialogParameters<UpdateProcesoRequest>
        {
            ShowTitle = false,
            ShowDismiss = false,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(95vw, 300px)"
        };

        var updateRequest = new UpdateProcesoRequest
        {
            Id = proceso.Id,
            Nombre = proceso.Nombre
        };

        var dialog = await DialogService.ShowDialogAsync<ProcesoFormDialog>(updateRequest, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled && result.Data is ProcesoDto updatedProceso)
        {
            var index = activeProcesos.FindIndex(p => p.Id == updatedProceso.Id);
            if (index >= 0) activeProcesos[index] = updatedProceso;
            activeProcesos = activeProcesos.ToList();
            StateHasChanged();
        }
    }

    private async Task DeleteProceso(ProcesoDto proceso)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar el proceso '<b>{proceso.Nombre}</b>'?"),
                Icon = new Size24.Delete(),
                IconColor = Color.Warning
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(new DeleteProcesoRequest(proceso.Id, Permanent: false));
            if (result.IsSuccess)
            {
                activeProcesos.RemoveAll(p => p.Id == proceso.Id);
                activeProcesos = activeProcesos.ToList();
                StateHasChanged();
            }
        }
    }

    private async Task EliminarSeleccionados()
    {
        if (procesoDataGrid == null || !procesoDataGrid.HasSelection) return;

        var seleccionados = procesoDataGrid.SelectedItems;
        var count = seleccionados.Count();

        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación masiva",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar <b>{count}</b> procesos?"),
                Icon = new Size24.Delete(),
                IconColor = Color.Error
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var ids = seleccionados.Select(p => p.Id).ToList();
            var result = await Mediator.Send(new DeleteProcesosRequest(ids));

            if (result.IsSuccess)
            {
                activeProcesos.RemoveAll(p => ids.Contains(p.Id));
                activeProcesos = activeProcesos.ToList();
                StateHasChanged();
            }
            else
            {
                await DialogService.ShowErrorAsync("Error", string.Join(", ", result.Errors));
            }
        }
    }

    private async Task ViewProcesoDetails(ProcesoDto proceso)
    {
        var parameters = new DialogParameters<ProcesoDto>
        {
            Title = "Detalles del proceso",
            PrimaryAction = null,
            SecondaryAction = null,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(90vw, 500px)"
        };
        await DialogService.ShowDialogAsync<ProcesoDetailDialog>(proceso, parameters);
    }

    #endregion

    #region Operaciones con papelera

    private void OnSelectedDeletedIdsChanged(HashSet<int> ids)
    {
        selectedDeletedProcesoIds = ids;
        StateHasChanged();
    }

    private async Task RestoreSingleProceso(ProcesoDto proceso)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración",
                MarkupMessage = new MarkupString($"¿Restaurar el proceso '<b>{proceso.Nombre}</b>'?"),
                Icon = new Size24.ArrowSync(),
                IconColor = Color.Info
            },
            PrimaryAction = "Restaurar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(new RestoreProcesoRequest(new List<int> { proceso.Id }));
            if (result.IsSuccess)
            {
                deletedProcesos.RemoveAll(p => p.Id == proceso.Id);
                activeProcesos.Add(proceso);
                activeProcesos = activeProcesos.ToList();
                deletedProcesos = deletedProcesos.ToList();
                if (selectedDeletedProcesoIds.Contains(proceso.Id))
                    selectedDeletedProcesoIds.Remove(proceso.Id);
                StateHasChanged();
            }
        }
    }

    private async Task RestoreSelectedProcesos()
    {
        if (!selectedDeletedProcesoIds.Any()) return;

        var count = selectedDeletedProcesoIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración múltiple",
                MarkupMessage = new MarkupString($"¿Restaurar <b>{count}</b> proceso(s) seleccionado(s)?"),
                Icon = new Size24.ArrowSync(),
                IconColor = Color.Info
            },
            PrimaryAction = "Restaurar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var ids = selectedDeletedProcesoIds.ToList();
            var result = await Mediator.Send(new RestoreProcesoRequest(ids));
            if (result.IsSuccess)
            {
                var procesosToRestore = deletedProcesos.Where(p => ids.Contains(p.Id)).ToList();
                activeProcesos.AddRange(procesosToRestore);
                deletedProcesos.RemoveAll(p => ids.Contains(p.Id));

                activeProcesos = activeProcesos.ToList();
                deletedProcesos = deletedProcesos.ToList();
                selectedDeletedProcesoIds.Clear();
                StateHasChanged();
            }
        }
    }

    private async Task DeleteSelectedPermanently()
    {
        if (!selectedDeletedProcesoIds.Any()) return;

        var count = selectedDeletedProcesoIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminación permanente masiva",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente <b>{count}</b> proceso(s) seleccionado(s)? Esta acción no se puede deshacer."),
                Icon = new Size24.Delete(),
                IconColor = Color.Error
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var ids = selectedDeletedProcesoIds.ToList();
            var result = await Mediator.Send(new DeleteProcesosRequest(ids, Permanent: true));
            if (result.IsSuccess)
            {
                deletedProcesos.RemoveAll(p => ids.Contains(p.Id));
                selectedDeletedProcesoIds.Clear();
                deletedProcesos = deletedProcesos.ToList();
                StateHasChanged();
            }
            else
            {
                await DialogService.ShowErrorAsync("Error", string.Join(", ", result.Errors));
            }
        }
    }

    private async Task DeletePermanently(ProcesoDto proceso)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminar permanentemente",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente '<b>{proceso.Nombre}</b>'? Esta acción no se puede deshacer."),
                Icon = new Size24.Delete(),
                IconColor = Color.Error
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"
        });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(new DeleteProcesoRequest(proceso.Id, Permanent: true));
            if (result.IsSuccess)
            {
                deletedProcesos.RemoveAll(p => p.Id == proceso.Id);
                deletedProcesos = deletedProcesos.ToList();
                StateHasChanged();
            }
        }
    }

    #endregion

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    private async Task ExportProcesos()
    {
        var excelData = await ExcelExportService.ExportAllProcesosDetailedToExcelAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Procesos.xlsx", Convert.ToBase64String(excelData));
    }
    private async Task ExportProcesosPdf()
    {
            var pdfData = await ExcelExportService.ExportAllProcesosDetailedToPdfAsync();
            await JSRuntime.InvokeVoidAsync("downloadFile", "Procesos.pdf", Convert.ToBase64String(pdfData)); 
    }

}