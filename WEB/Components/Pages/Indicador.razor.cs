using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Icons.Filled;
using Microsoft.JSInterop;
using WEB.Common;
using WEB.Components.Administrador.Indicador;
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
    

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _cancellationToken = new CancellationTokenSource();
            await LoadIndicadoresActivos();
        }
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
        selectedDeletedIds = deletedGridItems
            .Where(i => i.IsSelected)
            .Select(i => i.Id)
            .ToHashSet();
    
        StateHasChanged(); 
        await Task.CompletedTask;
    }
    
    private async Task ExportIndicadores()
    {
        var excelData = await ExcelExportService.ExportIndicadoresToExcelAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Indicadores.xlsx", Convert.ToBase64String(excelData));
    }
    private async Task ExportIndicadoresPdf()
    {
        var pdfData = await ExcelExportService.ExportIndicadoresToPdfAsync();
        await JSRuntime.InvokeVoidAsync("downloadFile", "Indicadores.pdf", Convert.ToBase64String(pdfData)); 
    }
}