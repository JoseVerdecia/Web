using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Icons.Filled;
using Microsoft.JSInterop;
using WEB.Components.Administrador.Objetivo;
using WEB.Features.Objetivo.Delete;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;
using WEB.Features.Objetivo.Restore;

namespace WEB.Components.Pages;

public partial class Objetivo : ComponentBase
{
    private CancellationTokenSource _cts = new();
    
    private List<ObjetivoDto> activeObjetivos = new();
    private List<ObjetivoDto> deletedObjetivos = new();
    private HashSet<int> selectedDeletedObjetivoIds = new();
    private ObjetivoDataGrid objetivoDataGrid = default!;
    
    private bool isLoadingActivos = true;
    private bool isLoadingPapelera = false;
    
    private string ActiveTabId = "tab-activos";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender) 
            await LoadActiveObjetivos();
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == "tab-papelera" && !deletedObjetivos.Any() && !isLoadingPapelera)
            await LoadDeletedObjetivos();
    }

    private async Task OnGridSelectionChanged()
    {
        StateHasChanged();
        await Task.CompletedTask;
    }

    #region Carga de datos

    private async Task LoadActiveObjetivos()
    {
        isLoadingActivos = true;
        StateHasChanged();
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allObjetivos = new List<ObjetivoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllObjetivosRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value.Items != null)
                {
                    allObjetivos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allObjetivos.Count < totalCount);

            activeObjetivos = allObjetivos;
        }
        finally
        {
            isLoadingActivos = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadDeletedObjetivos()
    {
        isLoadingPapelera = true;
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allObjetivos = new List<ObjetivoDto>();

            do
            {
                var result = await Mediator.Send(new GetAllObjetivosSoftDeleteRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value.Items != null)
                {
                    allObjetivos.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allObjetivos.Count < totalCount);

            deletedObjetivos = allObjetivos;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync("Error al cargar objetivos eliminados", ex.Message);
        }
        finally
        {
            isLoadingPapelera = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Operaciones con objetivos activos

    private async Task OpenCreateDialog()
    {
        var parameters = new DialogParameters<CreateObjetivoRequest>
        {
            ShowTitle = false,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(95vw, 600px)",
            ShowDismiss = false,
        };

        var dialog = await DialogService.ShowDialogAsync<ObjetivoFormDialog>(new CreateObjetivoRequest(), parameters);
        var result = await dialog.Result;
        if (!result.Cancelled && result.Data is ObjetivoDto newObjetivo)
        {
            activeObjetivos = activeObjetivos.Append(newObjetivo).ToList();
            StateHasChanged();
        }
    }

    private async Task OpenEditDialog(ObjetivoDto objetivo)
    {
        var parameters = new DialogParameters<UpdateObjetivoRequest>
        {
            ShowTitle = false,
            ShowDismiss = false,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(95vw, 600px)"
        };

        var updateRequest = new UpdateObjetivoRequest
        {
            Id = objetivo.Id,
            Nombre = objetivo.Nombre,
            NumeroObjetivo = objetivo.NumeroObjetivo
        };

        var dialog = await DialogService.ShowDialogAsync<ObjetivoFormDialog>(updateRequest, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled && result.Data is ObjetivoDto updatedObjetivo)
        {
            var index = activeObjetivos.FindIndex(o => o.Id == updatedObjetivo.Id);
            if (index >= 0) activeObjetivos[index] = updatedObjetivo;
            activeObjetivos = activeObjetivos.ToList();
            StateHasChanged();
        }
    }

    private async Task DeleteObjetivo(ObjetivoDto objetivo)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar el objetivo '<b>{objetivo.Nombre}</b>'?"),
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
            var result = await Mediator.Send(new DeleteObjetivoRequest(objetivo.Id, Permanent: false));
            if (result.IsSuccess)
            {
                activeObjetivos.RemoveAll(o => o.Id == objetivo.Id);
                activeObjetivos = activeObjetivos.ToList();
                StateHasChanged();
            }
        }
    }

    private async Task EliminarSeleccionados()
    {
        if (objetivoDataGrid == null || !objetivoDataGrid.HasSelection) return;

        var seleccionados = objetivoDataGrid.SelectedItems;
        var count = seleccionados.Count();

        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación masiva",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar <b>{count}</b> objetivos?"),
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
            var ids = seleccionados.Select(a => a.Id).ToList();
            var result = await Mediator.Send(new DeleteObjetivosRequest(ids));

            if (result.IsSuccess)
            {
                activeObjetivos.RemoveAll(o => ids.Contains(o.Id));
                activeObjetivos = activeObjetivos.ToList();
                StateHasChanged();
            }
            else
            {
                await DialogService.ShowErrorAsync("Error", string.Join(", ", result.Errors));
            }
        }
    }

    private async Task ViewObjetivoDetails(ObjetivoDto objetivo)
    {
        var parameters = new DialogParameters<ObjetivoDto>
        {
            Title = "Detalles del objetivo",
            PrimaryAction = null,
            SecondaryAction = null,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(90vw, 500px)"
        };
        await DialogService.ShowDialogAsync<ObjetivoDetailDialog>(objetivo, parameters);
    }

    #endregion

    #region Operaciones con papelera

    private void OnSelectedDeletedIdsChanged(HashSet<int> ids)
    {
        selectedDeletedObjetivoIds = ids;
        StateHasChanged();
    }

    private async Task RestoreSingleObjetivo(ObjetivoDto objetivo)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración",
                MarkupMessage = new MarkupString($"¿Restaurar el objetivo '<b>{objetivo.Nombre}</b>'?"),
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
            var result = await Mediator.Send(new RestoreObjetivosRequest(new List<int> { objetivo.Id }));
            if (result.IsSuccess)
            {
                deletedObjetivos.RemoveAll(o => o.Id == objetivo.Id);
                activeObjetivos.Add(objetivo);
                activeObjetivos = activeObjetivos.ToList();
                deletedObjetivos = deletedObjetivos.ToList();
                if (selectedDeletedObjetivoIds.Contains(objetivo.Id))
                    selectedDeletedObjetivoIds.Remove(objetivo.Id);
                StateHasChanged();
            }
        }
    }

    private async Task RestoreSelectedObjetivos()
    {
        if (!selectedDeletedObjetivoIds.Any()) return;

        var count = selectedDeletedObjetivoIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración múltiple",
                MarkupMessage = new MarkupString($"¿Restaurar <b>{count}</b> objetivo(s) seleccionado(s)?"),
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
            var ids = selectedDeletedObjetivoIds.ToList();
            var result = await Mediator.Send(new RestoreObjetivosRequest(ids));
            if (result.IsSuccess)
            {
                var itemsToRestore = deletedObjetivos.Where(o => ids.Contains(o.Id)).ToList();
                activeObjetivos.AddRange(itemsToRestore);
                deletedObjetivos.RemoveAll(o => ids.Contains(o.Id));

                activeObjetivos = activeObjetivos.ToList();
                deletedObjetivos = deletedObjetivos.ToList();
                selectedDeletedObjetivoIds.Clear();
                StateHasChanged();
            }
        }
    }

    private async Task DeletePermanently(ObjetivoDto objetivo)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminar permanentemente",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente '<b>{objetivo.Nombre}</b>'? Esta acción no se puede deshacer."),
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
            var result = await Mediator.Send(new DeleteObjetivoRequest(objetivo.Id, Permanent: true));
            if (result.IsSuccess)
            {
                deletedObjetivos.RemoveAll(o => o.Id == objetivo.Id);
                deletedObjetivos = deletedObjetivos.ToList();
                StateHasChanged();
            }
        }
    }

    private async Task DeleteSelectedPermanently()
    {
        if (!selectedDeletedObjetivoIds.Any()) return;

        var count = selectedDeletedObjetivoIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminación permanente masiva",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente <b>{count}</b> objetivo(s) seleccionado(s)? Esta acción no se puede deshacer."),
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
            var ids = selectedDeletedObjetivoIds.ToList();
            var result = await Mediator.Send(new DeleteObjetivosRequest(ids, Permanent: true));
            if (result.IsSuccess)
            {
                deletedObjetivos.RemoveAll(o => ids.Contains(o.Id));
                selectedDeletedObjetivoIds.Clear();
                deletedObjetivos = deletedObjetivos.ToList();
                StateHasChanged();
            }
            else
            {
                await DialogService.ShowErrorAsync("Error", string.Join(", ", result.Errors));
            }
        }
    }

    #endregion
    
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    
    private async Task ExportObjetivos()
    {
        var excelData = await ExcelExportService.ExportAllObjetivosDetailedToExcelAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Objetivos.xlsx", Convert.ToBase64String(excelData));
    }
    
    private async Task ExportObjetivosPdf()
    {
        var pdfData = await ExcelExportService.ExportAllObjetivosDetailedToPdfAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Objetivos.pdf", Convert.ToBase64String(pdfData));
    }
}