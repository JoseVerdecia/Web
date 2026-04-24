using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Icons.Filled;
using WEB.Common;
using WEB.Components.Administrador.Indicador;
using WEB.Core.Extensions;
using WEB.Core.Mediator;
using WEB.Enums;
using WEB.Features.Indicador.Delete;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.GetAll;
using WEB.Features.Indicador.Restore;

namespace WEB.Components.Pages;

public partial class Indicador : ComponentBase
{
      private CancellationTokenSource _cancellationToken = new();

    // Datos activos
    private List<IndicadorDto> indicadoresDto = new();
    private List<IndicadorDisplayItem> gridItems = new();
    private IndicadorDisplayItem? selectedIndicador;
    private IndicadorDataGrid indicadorDataGrid = default!;
    private IndicadorDeletedDataGrid deletedDataGrid = default!;
    // Datos eliminados
    private List<IndicadorDisplayItem> deletedGridItems = new();
    private HashSet<int> selectedDeletedIds = new();

    // Estados de carga
    private bool isLoading = false;
    private bool isLoadingPapelera = false;
    private string? errorMessage;

    // Pestaña activa
    private string ActiveTabId = "tab-activos";
    

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine(">>> OnInitializedAsync ejecutado");
        _cancellationToken = new CancellationTokenSource();
        await LoadIndicadoresActivos();
    }

    private async Task HandleTabChange(FluentTab tab)
    {
        if (tab.Id == "tab-papelera" && !deletedGridItems.Any() && !isLoadingPapelera)
        {
            await LoadIndicadoresEliminados();
        }
    }

    #region Carga de Datos

    private async Task LoadIndicadoresActivos()
    {
        if (isLoading || gridItems.Any()) return;
        isLoading = true;
        StateHasChanged();
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allIndicadores = new List<IndicadorDto>();

            do
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;
                
                var result = await Mediator.Send(
                    new GetAllIndicadoresRequest(currentPage, pageSize),
                    _cancellationToken.Token);

                if (result.IsSuccess && result.Value?.Items != null)
                {
                    allIndicadores.AddRange(result.Value.Items);
                    totalCount = result.Value.TotalCount;
                    currentPage++;
                }
                else
                {
                    errorMessage = "No se pudieron obtener todos los indicadores.";
                    break;
                }
            } while (allIndicadores.Count < totalCount);

            indicadoresDto = allIndicadores;
            gridItems = allIndicadores.Select(IndicadorDisplayItem.FromIndicadorDto).ToList();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            errorMessage = $"Error al cargar los indicadores: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadIndicadoresEliminados()
    {
        isLoadingPapelera = true;
        try
        {
            const int pageSize = 50;
            var currentPage = 1;
            var totalCount = 0;
            var allIndicadores = new List<IndicadorDto>();

            do
            {
                var result = await Mediator.Send(
                    new GetAllIndicadoresSoftDeleteRequest(currentPage, pageSize),
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

            deletedGridItems = allIndicadores.Select(dto => 
            {
                var item = IndicadorDisplayItem.FromIndicadorDto(dto);
                item.IsSelected = false; 
                return item;
            }).ToList();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync("Error", $"No se pudieron cargar los indicadores eliminados: {ex.Message}");
        }
        finally
        {
            isLoadingPapelera = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Operaciones con Activos

    private void HandleRowSelected(IndicadorDisplayItem item)
    {
        selectedIndicador = item;
    }

    private async Task OpenCreateWizardDialog()
    {
        var width = "min(110vw, 850px)";
        var parameters = new DialogParameters<CreateIndicadorRequest>
        {
            ShowTitle = false,
            PreventDismissOnOverlayClick = true,
            PrimaryAction = null,
            SecondaryAction = null,
            Modal = true,
            Width = width,
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
            indicadoresDto.Add(newIndicador);
            gridItems.Add(IndicadorDisplayItem.FromIndicadorDto(newIndicador));
            StateHasChanged();
        }
    }

    private async Task OpenEditDialog(IndicadorDisplayItem displayItem)
    {
        var indicadorDto = indicadoresDto.FirstOrDefault(i => i.Id == displayItem.Id);
        if (indicadorDto == null) return;

        Dictionary<int, string>? metasPorArea = null;
        if (indicadorDto.Areas.Any())
        {
            metasPorArea = indicadorDto.Areas
                .Where(ia => ia.AreaId > 0)
                .ToDictionary(ia => ia.AreaId, ia => ia.MetaCumplir ?? string.Empty);
        }

        var parameters = new DialogParameters<UpdateIndicadorRequest>
        {
            ShowTitle = false,
            PreventDismissOnOverlayClick = true,
            PrimaryAction = null,
            SecondaryAction = null,
            Modal = true,
            Width = "min(95vw, 850px)",
            ShowDismiss = false,
        };

        var updateRequest = new UpdateIndicadorRequest
        {
            Id = indicadorDto.Id,
            Nombre = indicadorDto.Nombre,
            MetaCumplir = indicadorDto.MetaCumplir,
            MetaReal = indicadorDto.MetaReal,
            Origen = indicadorDto.Origen,
            Tipo = indicadorDto.Tipo,
            Observacion = indicadorDto.Observacion,
            ProcesoId = indicadorDto.Proceso?.Id ?? 0,
            ObjetivoIds = indicadorDto.Objetivos?.Select(o => o.Id).ToList(),
            MetaCumplirPorArea = metasPorArea
        };

        var dialog = await DialogService.ShowDialogAsync<IndicadorWizardDialog>(updateRequest, parameters);
        var result = await dialog.Result;

        if (!result.Cancelled && result.Data is IndicadorDto updatedIndicador)
        {
            var indexDto = indicadoresDto.FindIndex(i => i.Id == updatedIndicador.Id);
            if (indexDto >= 0) indicadoresDto[indexDto] = updatedIndicador;

            var indexGrid = gridItems.FindIndex(i => i.Id == updatedIndicador.Id);
            if (indexGrid >= 0)
            {
                var newItem = IndicadorDisplayItem.FromIndicadorDto(updatedIndicador);
                gridItems[indexGrid] = newItem;

                if (selectedIndicador?.Id == updatedIndicador.Id)
                {
                    selectedIndicador = null;
                    StateHasChanged();
                    selectedIndicador = newItem;
                }
            }
            StateHasChanged();
        }
    }

    private async Task DeleteIndicador(IndicadorDisplayItem displayItem)
    {
        var indicadorDto = indicadoresDto.FirstOrDefault(i => i.Id == displayItem.Id);
        if (indicadorDto == null) return;

        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación",
                MarkupMessage = new MarkupString($"¿Estás seguro de eliminar el indicador '{indicadorDto.Nombre}'?"),
                Icon = new Size24.Delete(),
                IconColor = Color.Warning
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar",
            Width = "min(200vw, 400px)"
        });

        var dialogResult = await dialog.Result;

        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(new DeleteIndicadorRequest(indicadorDto.Id, Permanent: false));
            if (result.IsSuccess)
            {
                indicadoresDto.RemoveAll(i => i.Id == indicadorDto.Id);
                gridItems.RemoveAll(i => i.Id == indicadorDto.Id);

                if (selectedIndicador?.Id == indicadorDto.Id)
                    selectedIndicador = null;

                StateHasChanged();
            }
        }
    }

    private async Task ViewIndicadorDetails(IndicadorDisplayItem displayItem)
    {
        var indicadorDto = indicadoresDto.FirstOrDefault(i => i.Id == displayItem.Id);
        if (indicadorDto == null) return;

        var parameters = new DialogParameters<IndicadorDto>
        {
            Title = "Detalles del Indicador",
            PrimaryAction = null,
            SecondaryAction = null,
            PreventDismissOnOverlayClick = true,
            Modal = true,
            Width = "min(90vw, 500px)"
        };

        await DialogService.ShowDialogAsync<IndicadorDetailDialog>(indicadorDto, parameters);
    }

    #endregion

    #region Operaciones con Papelera

    private void OnSelectedDeletedIdsChanged(HashSet<int> ids)
    {
        selectedDeletedIds = ids;
        StateHasChanged();
    }

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
            SecondaryAction = "Cancelar",
            Width = "min(200vw, 300px)"
        });

        var request = new RestoreIndicadoresRequest(new List<int> { displayItem.Id });

        var dialogResult = await dialog.Result;
        if (!dialogResult.Cancelled)
        {
            var result = await Mediator.Send(request);
            if (result.IsSuccess)
            {
                deletedGridItems.RemoveAll(i => i.Id == displayItem.Id);
                await LoadIndicadoresActivos();
                StateHasChanged();
            }
        }
    }

    private async Task RestoreSelectedIndicadores()
    {
        if (deletedDataGrid == null || !deletedDataGrid.HasSelection) 
            return;

        var seleccionados = deletedDataGrid.SelectedItems;
        var count = seleccionados.Count;
        
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar restauración",
                MarkupMessage = new MarkupString($"¿Desea restaurar <b>{count}</b> indicador(es)?"),
                Icon = new Size24.ArrowSync(),
                IconColor = Color.Info
            },
            PrimaryAction = "Restaurar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"   
        });

        var dialogResult = await dialog.Result;
        if (dialogResult.Cancelled)
            return;

        var ids = seleccionados.Select(i => i.Id).ToList();
        var request = new RestoreIndicadoresRequest(ids);
        var result = await Mediator.Send(request);

        if (result.IsSuccess)
        {
            deletedGridItems.RemoveAll(i => ids.Contains(i.Id));
            await LoadIndicadoresActivos();
            StateHasChanged();
        }
        else
        {
            errorMessage = string.Join(", ", result.Errors);
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

    #endregion

    public void Dispose()
    {
        _cancellationToken?.Cancel();
        _cancellationToken?.Dispose();
    }
    
    private async Task EliminarSeleccionados()
    {
        if (indicadorDataGrid == null || !indicadorDataGrid.HasSelection)
            return;

        var seleccionados = indicadorDataGrid.SelectedItems;
        var count = seleccionados.Count();

        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>()
        {
            Content = new()
            {
                Title = "Confirmar eliminación masiva",
                MarkupMessage = new MarkupString($"¿Estás seguro de que deseas eliminar <b>{count}</b> indicadores?"),
                Icon = new Size24.Delete(),
                IconColor = Color.Error
            },
            PrimaryAction = "Eliminar",
            SecondaryAction = "Cancelar",
            Width = "min(90vw, 400px)"   
        });

        var dialogResult = await dialog.Result;
        if (dialogResult.Cancelled)
            return;

        var ids = seleccionados.Select(i => i.Id).ToList();
        var result = await Mediator.Send(new DeleteIndicadoresRequest(ids));

        if (result.IsSuccess)
        {
           
            indicadoresDto.RemoveAll(i => ids.Contains(i.Id));
            gridItems.RemoveAll(i => ids.Contains(i.Id));

           
            if (selectedIndicador != null && ids.Contains(selectedIndicador.Id))
                selectedIndicador = null;
            
            gridItems = gridItems.ToList();
            StateHasChanged();
        }
        else
        {
            errorMessage = string.Join(", ", result.Errors);
        }
    }
    
    private async Task DeleteSelectedPermanently()
    {
        if (!selectedDeletedIds.Any()) return;

        var count = selectedDeletedIds.Count;
        var dialog = await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
        {
            Content = new()
            {
                Title = "Eliminación permanente masiva",
                MarkupMessage = new MarkupString($"¿Eliminar definitivamente <b>{count}</b> indicador(es)? Esta acción no se puede deshacer."),
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
            var ids = selectedDeletedIds.ToList();
            var result = await Mediator.Send(new DeleteIndicadoresRequest(ids, Permanent: true));
            if (result.IsSuccess)
            {
                deletedGridItems.RemoveAll(i => ids.Contains(i.Id));
                selectedDeletedIds.Clear();
                deletedGridItems = deletedGridItems.ToList();
                StateHasChanged();
            }
            else
            {
                await DialogService.ShowErrorAsync("Error", string.Join(", ", result.Errors));
            }
        }
    }
    
    private async Task OnGridSelectionChanged()
    {
        
        await Task.CompletedTask;
    }
    private async Task OnDeletedGridSelectionChanged()
    {
       
        await Task.CompletedTask;
    }
}