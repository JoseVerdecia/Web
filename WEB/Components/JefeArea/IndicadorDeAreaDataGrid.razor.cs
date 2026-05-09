using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.IndicadorDeArea.GetAll;
using WEB.Features.Notificacion.Crear;
using WEB.Features.Proceso.GetAll;


namespace WEB.Components.JefeArea;


public  partial class IndicadorDeAreaDataGrid : ComponentBase
{
    private bool _isLoading = true;
    private string? _errorMessage;
    private FluentDataGrid<IndicadorDeAreaDto>? _dataGrid;
    private bool _disposed;
    private GridItemsProvider<IndicadorDeAreaDto> _itemsProvider = default!;
    private bool _isInitializing = false;
    private bool _showSolicitudDialog;
    private string? _procesoFilter;
    private List<SelectOption<string?>> _procesoOptions = new();
    private IndicadorDeAreaDto? _indicadorSeleccionado;
    private string _nuevaMetaPropuesta = string.Empty;
    private string _mensajePersonalizado = string.Empty;

    protected override async void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _itemsProvider = async req =>
            {
                try
                {
                    var result = await Mediator.Send(new GetAllIndicadorDeAreaRequest(Page: 1, PageSize: 100));
                    if (result.IsSuccess && result.Value?.Items != null)
                    {
                        var items = result.Value.Items.ToList();

                        if (!string.IsNullOrWhiteSpace(_procesoFilter))
                            items = items.Where(i => string.Equals(i.ProcesoNombre, _procesoFilter, StringComparison.OrdinalIgnoreCase)).ToList();

                        _errorMessage = null;
                        return GridItemsProviderResult.From(items: items, totalItemCount: items.Count);
                    }
                    _errorMessage = "No se pudieron cargar los indicadores.";
                }
                catch (Exception ex)
                {
                    _errorMessage = $"Error: {ex.Message}";
                }
                return GridItemsProviderResult.From(items: new List<IndicadorDeAreaDto>(), totalItemCount: 0);
            };

            IndicadorUpdateState.OnIndicadorUpdated += OnMetaUpdated;
        
            await CargarOpcionesProcesosAsync();
            StateHasChanged();
        }
    }

    private async Task CargarOpcionesProcesosAsync()
    {
        try
        {
            var procesosResult = await Mediator.Send(new GetAllProcesosRequest(Page: 1, PageSize: 100));
            if (procesosResult.IsSuccess && procesosResult.Value?.Items != null)
            {
                _procesoOptions = procesosResult.Value.Items
                    .Select(p => new SelectOption<string?>
                    {
                        Value = p.Nombre,
                        Text = p.Nombre
                    })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Error al cargar procesos: {ex.Message}");
        }
    }
    
    private async Task ClearProcesoFilter()
    {
        _procesoFilter = null;
        await RefrescarGrid();
    }
    
    private async Task RefrescarGrid()
    {
        if (_dataGrid != null)
            await _dataGrid.RefreshDataAsync();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_isInitializing)
        {
            _isInitializing = true;
            await Task.Delay(100);
            if (_dataGrid != null)
            {
                await _dataGrid.RefreshDataAsync();
                _isLoading = false;
                StateHasChanged();
            }
            _isInitializing = false;
        }
    }
    
    private bool _isRefreshing = false;
    private async void OnMetaUpdated()
    {
        if (_disposed) return;
        await InvokeAsync(async () =>
        {
            _isLoading = true;
            StateHasChanged();
            _isRefreshing = true;
            if (_dataGrid != null)
            {
                await _dataGrid.RefreshDataAsync();
            }
            _isLoading = false;
            _isRefreshing = false;
            StateHasChanged();
        });
    }

    private async Task OpenEditMetaRealDialog(IndicadorDeAreaDto item)
    {
        var parameters = new DialogParameters<IndicadorDeAreaDto>
        {
            Title = "Meta Real",
            PrimaryAction = null,
            SecondaryAction = null,
            Modal = true,
            Width = "800px"
        };

        var dialog = await DialogService.ShowDialogAsync<MetaRealDialog>(item, parameters);
        var result = await dialog.Result;

        if (!result.Cancelled && result.Data is IndicadorDeAreaDto updatedDto)
        {
            if (_dataGrid != null)
            {
                await _dataGrid.RefreshDataAsync();
            }
        }
    }

    private void AbrirSolicitudDialog(IndicadorDeAreaDto indicador)
    {
        _indicadorSeleccionado = indicador;
        _nuevaMetaPropuesta = indicador.MetaCumplir ?? string.Empty;
        _mensajePersonalizado = string.Empty;
        _showSolicitudDialog = true;
    }

    private async Task EnviarSolicitudAsync()
    {
        if (_indicadorSeleccionado == null || string.IsNullOrWhiteSpace(_nuevaMetaPropuesta))
            return;

        var user = await CurrentUser.GetUserAsync();
        if (user == null) return;

        var result = await Mediator.Send(new CrearSolicitudCambioMetaRequest(
            IndicadorDeAreaId: _indicadorSeleccionado.Id,
            NuevaMetaPropuesta: _nuevaMetaPropuesta,
            MensajePersonalizado: _mensajePersonalizado,
            RemitenteId: user.Id
        ));

        if (result.IsSuccess)
        {
            NotificationService.ShowSuccess("Solicitud enviada correctamente.");
            _showSolicitudDialog = false;
        }
        else
        {
            NotificationService.ShowError(string.Join(", ", result.Errors.Select(e => e.Message)));
        }
    }

    public void Dispose()
    {
        _disposed = true;
        IndicadorUpdateState.OnIndicadorUpdated -= OnMetaUpdated;
    }
    
}