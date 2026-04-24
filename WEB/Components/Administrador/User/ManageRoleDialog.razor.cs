using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Mediator;
using WEB.Core.Services;
using WEB.Data;
using WEB.Features.Area.Assign;
using WEB.Features.Area.Available;
using WEB.Features.Area.Denegar;
using WEB.Features.Proceso.Assign;
using WEB.Features.Proceso.Available;
using WEB.Features.Proceso.Denegar;
using WEB.Features.Users.Dto;

namespace WEB.Components.Administrador.User;

public partial class ManageRoleDialog : ComponentBase, IDialogContentComponent
{
    [Parameter] public EventCallback OnRoleChanged { get; set; }

    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    [Parameter] public UserDto? Content { get; set; }

    private enum DialogStep { Main, SelectArea, SelectProceso }
    private DialogStep _step = DialogStep.Main;
    private bool _isProcessing;
    private string? _errorMessage;
    private AvailableUserDto? _selectedItem;
    private List<AvailableUserDto>? _availableItems;
    
    
    // Nuevo campo para la búsqueda en listas
    private string _searchText = string.Empty;
    
    private static Appearance GetBadgeAppearance(string role) => role switch
    {
        AppRoles.JefeArea => Appearance.Accent,
        AppRoles.JefeProceso => Appearance.Accent,
        AppRoles.UsuarioNormal => Appearance.Accent,
        _ => Appearance.Neutral
    };
    
    private IEnumerable<AvailableUserDto> FilteredItems
    {
        get
        {
            if (_availableItems == null)
                return Enumerable.Empty<AvailableUserDto>();

            if (string.IsNullOrWhiteSpace(_searchText))
                return _availableItems;

            return _availableItems
                .Where(x => x.Nombre.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private void GoBack()
    {
        _step = DialogStep.Main;
        _selectedItem = null;
        _availableItems = null;
        _errorMessage = null;
        _searchText = string.Empty; // Limpiar búsqueda al volver
    }

    private async Task DenegarAreaAsync()
    {
        if (Content is null || !Content.AreaId.HasValue || Content.AreaId.Value <= 0) 
            return;

        _isProcessing = true;
        _errorMessage = null;
        StateHasChanged();

        var result = await Mediator.Send(new DenegarResponsableAreaRequest(Content.Id, Content.AreaId));

        _isProcessing = false;

        if (result.IsSuccess)
        {
            Notification.ShowSuccess($"Se ha removido a {Content.FullName} como Jefe de Área.");
            await OnRoleChanged.InvokeAsync();
            await Dialog.CloseAsync(DialogResult.Ok(true));
        }
        else
        {
            _errorMessage = $"No se pudo remover la responsabilidad del área: {string.Join(',', result.Errors.Select(e => e.Message))}";
            StateHasChanged();
        }
    }

    private async Task DenegarProcesoAsync()
    {
        if (Content is null || Content.ProcesoId <= 0) return;

        _isProcessing = true;
        _errorMessage = null;
        StateHasChanged();

        var result = await Mediator.Send(new DenegarResponsableRequest(Content.Id, Content.ProcesoId));

        _isProcessing = false;

        if (result.IsSuccess)
        {
            Notification.ShowSuccess($"Se ha removido a {Content.FullName} como Jefe de Proceso.");
            await OnRoleChanged.InvokeAsync();
            await Dialog.CloseAsync(DialogResult.Ok(true));
        }
        else
        {
            _errorMessage = $"No se pudo remover la responsabilidad del proceso: {string.Join(',', result.Errors.Select(e => e.Message))}";
            StateHasChanged();
        }
    }

    private async Task LoadAvailableAreasAsync()
    {
        if (Content?.Role == AppRoles.JefeProceso)
        {
            _errorMessage = $"Este usuario ya es {AppRoles.JefeProceso}. Debe denegar su responsabilidad actual antes de asignarle un área.";
            StateHasChanged();
            return;
        }

        _step = DialogStep.SelectArea;
        _selectedItem = null;
        _availableItems = null;
        _errorMessage = null;
        _searchText = string.Empty;
        _isProcessing = true;
        StateHasChanged();

        var resultAvailableAreas = await Mediator.Send(new GetAvailableAreasRequest());
        _availableItems = resultAvailableAreas.Value;

        _isProcessing = false;
        StateHasChanged();
    }

    private async Task LoadAvailableProcesosAsync()
    {
        if (Content?.Role == AppRoles.JefeArea)
        {
            _errorMessage = $"Este usuario ya es {AppRoles.JefeArea}. Debe denegar su responsabilidad actual antes de asignarle un proceso.";
            StateHasChanged();
            return;
        }

        _step = DialogStep.SelectProceso;
        _selectedItem = null;
        _availableItems = null;
        _errorMessage = null;
        _searchText = string.Empty;
        _isProcessing = true;
        StateHasChanged();
        
        var resultAvailableProcesos = await Mediator.Send(new GetAvailableProcesosRequest());
        _availableItems = resultAvailableProcesos.Value;
        
        _isProcessing = false;
        StateHasChanged();
    }

    // Eliminado el método OnItemSelected (ya no se usa con bind-SelectedOption)

    private async Task ConfirmAssignmentAsync()
    {
        if (_selectedItem is null || Content is null) return;

        _isProcessing = true;
        _errorMessage = null;
        StateHasChanged();

        bool success = false;

        try
        {
            if (_step == DialogStep.SelectArea)
            {
                if (Content.Role == AppRoles.JefeArea && Content.AreaId > 0)
                {
                    var denyResult = await Mediator.Send(new DenegarResponsableAreaRequest(Content.Id, Content.AreaId));
                    if (denyResult.IsFailure)
                    {
                        _errorMessage = $"No se pudo remover la responsabilidad previa del área: {string.Join(',', denyResult.Errors.Select(e => e.Message))}";
                        _isProcessing = false;
                        StateHasChanged();
                        return;
                    }
                }

                var result = await Mediator.Send(new AsignarResponsableAreaRequest(Content.Id, _selectedItem.Id));
             
                if (result.IsFailure)
                    _errorMessage = $"Error al asignar responsable del área. {string.Join(',', result.Errors.Select(e => e.Message))}";
                else
                    success = result.IsSuccess;
            }
            else if (_step == DialogStep.SelectProceso)
            {
                if (Content.Role == AppRoles.JefeProceso && Content.ProcesoId > 0)
                {
                    var denyResult = await Mediator.Send(new DenegarResponsableRequest(Content.Id, Content.ProcesoId));
                    
                    if (denyResult.IsFailure)
                    {
                        _errorMessage = $"No se pudo remover la responsabilidad previa del proceso: {string.Join(',', denyResult.Errors.Select(e => e.Message))}";
                        _isProcessing = false;
                        StateHasChanged();
                        return;
                    }
                }

                var result = await Mediator.Send(new AsignarResponsableProcesoRequest(Content.Id, _selectedItem.Id));
               
                if (result.IsFailure)
                    _errorMessage = $"Error al asignar responsable del proceso. {string.Join(',', result.Errors.Select(e => e.Message))}";
                else
                    success = result.IsSuccess;
            }
        }
        catch (Exception)
        {
            _errorMessage = "Ocurrió un error inesperado al procesar la asignación.";
            _isProcessing = false;
            StateHasChanged();
            return;
        }

        _isProcessing = false;

        if (success)
        {
            var rolName = _step == DialogStep.SelectArea ? "Jefe de Área" : "Jefe de Proceso";
            Notification.ShowSuccess($"{Content.FullName} ha sido asignado como {rolName} exitosamente.");
            await OnRoleChanged.InvokeAsync();
            await Dialog.CloseAsync(DialogResult.Ok(true));
        }
        else
        {
            StateHasChanged();
        }
    }

    private async Task CancelAsync()
    {
        await Dialog.CancelAsync();
    }
}