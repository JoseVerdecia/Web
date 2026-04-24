using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Icons.Filled;
using WEB.Components.Administrador.Area;
using WEB.Core.Mediator;
using WEB.Enums;
using WEB.Features.Area.Delete;
using WEB.Features.Area.Dto;
using WEB.Features.Area.GetAll;
using WEB.Features.Area.Restore;

namespace WEB.Components.Pages;

public partial class Area : ComponentBase
{
    private CancellationTokenSource _cts = new();

    private List<AreaDto> activeAreas = new();
    private List<AreaDto> deletedAreas = new();
    private HashSet<int> selectedDeletedAreaIds = new();
    private AreaDataGrid areaDataGrid = default!;

    private bool isLoadingActivas = true;
    private bool isLoadingPapelera = false;

    private string ActiveTabId = "tab-activas";

    private int Facultades => activeAreas.Count(a => a.Tipo == AreaTipo.Facultad);
    private int Municipios => activeAreas.Count(a => a.Tipo == AreaTipo.Municipio);

    protected override async Task OnInitializedAsync()
    {
        await LoadActiveAreas();
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == "tab-papelera" && !deletedAreas.Any() && !isLoadingPapelera)
            await LoadDeletedAreas();
    }

    private async Task OnGridSelectionChanged()
    {
        StateHasChanged();
        await Task.CompletedTask;
    }

    #region Carga de datos

    private async Task LoadActiveAreas()
    {
        isLoadingActivas = true;
        StateHasChanged();
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allAreas = new List<AreaDto>();

            do
            {
                var result = await Mediator.Send(new GetAllAreasRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value.Items != null)
                {
                    allAreas.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allAreas.Count < totalCount);

            activeAreas = allAreas;
        }
        finally
        {
            isLoadingActivas = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadDeletedAreas()
    {
        isLoadingPapelera = true;
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allAreas = new List<AreaDto>();

            do
            {
                var result = await Mediator.Send(new GetAllAreasSoftDeletedRequest(currentPage, pageSize), _cts.Token);
                if (result?.Value.Items != null)
                {
                    allAreas.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else break;
            } while (allAreas.Count < totalCount);

            deletedAreas = allAreas;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync("Error al cargar áreas eliminadas", ex.Message);
        }
        finally
        {
            isLoadingPapelera = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Operaciones con áreas activas

    private async Task OpenCreateDialog()
    {
        var parameters = new DialogParameters<CreateAreaRequest>
        {
            ShowTitle = false,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(95vw, 300px)",
            ShowDismiss = false,
        };

        var dialog = await DialogService.ShowDialogAsync<AreaFormDialog>(new CreateAreaRequest(), parameters);
        var result = await dialog.Result;
        if (!result.Cancelled && result.Data is AreaDto newArea)
        {
            activeAreas = activeAreas.Append(newArea).ToList();
            StateHasChanged();
        }
    }

    private async Task OpenEditDialog(AreaDto area)
    {
        var parameters = new DialogParameters<UpdateAreaRequest>
        {
            ShowTitle = false,
            ShowDismiss = false,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(95vw, 300px)"
        };

        var updateRequest = new UpdateAreaRequest
        {
            Id = area.Id,
            Nombre = area.Nombre,
            Tipo = area.Tipo
        };

        var dialog = await DialogService.ShowDialogAsync<AreaFormDialog>(updateRequest, parameters);
        var result = await dialog.Result;
        if (!result.Cancelled && result.Data is AreaDto updatedArea)
        {
            var index = activeAreas.FindIndex(a => a.Id == updatedArea.Id);
            if (index >= 0) activeAreas[index] = updatedArea;
            activeAreas = activeAreas.ToList();
            StateHasChanged();
        }
    }

    private async Task DeleteArea(AreaDto area)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar el área '<b>{area.Nombre}</b>'?"),
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
            var result = await Mediator.Send(new DeleteAreaRequest(area.Id, Permanent: false));
            if (result.IsSuccess)
            {
                activeAreas.RemoveAll(a => a.Id == area.Id);
                activeAreas = activeAreas.ToList();
                StateHasChanged();
            }
        }
    }

    private async Task EliminarSeleccionadas()
    {
        if (areaDataGrid == null || !areaDataGrid.HasSelection) return;

        var seleccionados = areaDataGrid.SelectedItems;
        var count = seleccionados.Count();

        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación masiva",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar <b>{count}</b> áreas?"),
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
            var result = await Mediator.Send(new DeleteAreasRequest(ids));

            if (result.IsSuccess)
            {
                activeAreas.RemoveAll(a => ids.Contains(a.Id));
                activeAreas = activeAreas.ToList();
                StateHasChanged();
            }
            else
            {
                await DialogService.ShowErrorAsync("Error", string.Join(", ", result.Errors));
            }
        }
    }

    private async Task ViewAreaDetails(AreaDto area)
    {
        var parameters = new DialogParameters<AreaDto>
        {
            Title = "Detalles del área",
            PrimaryAction = null,
            SecondaryAction = null,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(90vw, 500px)"
        };
        await DialogService.ShowDialogAsync<AreaDetailsDialog>(area, parameters);
    }

    #endregion

    #region Operaciones con papelera

    private void OnSelectedDeletedIdsChanged(HashSet<int> ids)
    {
        selectedDeletedAreaIds = ids;
        StateHasChanged();
    }

    private async Task RestoreSingleArea(AreaDto area)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración",
                MarkupMessage = new MarkupString($"¿Restaurar el área '<b>{area.Nombre}</b>'?"),
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
            var result = await Mediator.Send(new RestoreAreaRequest(new List<int> { area.Id }));
            if (result.IsSuccess)
            {
                deletedAreas.RemoveAll(a => a.Id == area.Id);
                activeAreas.Add(area);
                activeAreas = activeAreas.ToList();
                deletedAreas = deletedAreas.ToList();
                if (selectedDeletedAreaIds.Contains(area.Id))
                    selectedDeletedAreaIds.Remove(area.Id);
                StateHasChanged();
            }
        }
    }

    private async Task RestoreSelectedAreas()
    {
        if (!selectedDeletedAreaIds.Any()) return;

        var count = selectedDeletedAreaIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Confirmar restauración múltiple",
                MarkupMessage = new MarkupString($"¿Restaurar <b>{count}</b> área(s) seleccionada(s)?"),
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
            var ids = selectedDeletedAreaIds.ToList();
            var result = await Mediator.Send(new RestoreAreaRequest(ids));
            if (result.IsSuccess)
            {
                var areasToRestore = deletedAreas.Where(a => ids.Contains(a.Id)).ToList();
                activeAreas.AddRange(areasToRestore);
                deletedAreas.RemoveAll(a => ids.Contains(a.Id));

                activeAreas = activeAreas.ToList();
                deletedAreas = deletedAreas.ToList();
                selectedDeletedAreaIds.Clear();
                StateHasChanged();
            }
        }
    }

    private async Task DeletePermanently(AreaDto area)
    {
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminar permanentemente",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente '<b>{area.Nombre}</b>'? Esta acción no se puede deshacer."),
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
            var result = await Mediator.Send(new DeleteAreaRequest(area.Id, Permanent: true));
            if (result.IsSuccess)
            {
                deletedAreas.RemoveAll(a => a.Id == area.Id);
                deletedAreas = deletedAreas.ToList();
                StateHasChanged();
            }
        }
    }

    private async Task DeleteSelectedPermanently()
    {
        if (!selectedDeletedAreaIds.Any()) return;

        var count = selectedDeletedAreaIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminación permanente masiva",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente <b>{count}</b> área(s) seleccionada(s)? Esta acción no se puede deshacer."),
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
            var ids = selectedDeletedAreaIds.ToList();
            var result = await Mediator.Send(new DeleteAreasRequest(ids, Permanent: true));
            if (result.IsSuccess)
            {
                deletedAreas.RemoveAll(a => ids.Contains(a.Id));
                selectedDeletedAreaIds.Clear();
                deletedAreas = deletedAreas.ToList();
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
}