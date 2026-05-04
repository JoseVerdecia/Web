using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Services;
using WEB.Data;
using WEB.Features.Users.Delete;
using WEB.Features.Users.Dto;
using WEB.Features.Users.GetAll;

namespace WEB.Components.Administrador.User;

public partial class UserDataGrid : ComponentBase
{
    private FluentDataGrid<UserDto> dataGrid = default!;
    private List<string> roles = new();
    private List<Option<string>> roleOptions = new();
    private string? roleFilter = null;
    private bool isLoading = false;
    private PaginationState paginationState = new() { ItemsPerPage = 10 };
    private GridItemsProvider<UserDto> usersProvider = default!;
    
    private List<UserDto> users = new();
    private string nameFilter = string.Empty;
 
    private string? _previousRoleFilter;
  

    protected override async Task OnInitializedAsync()
    { 
        LoadRoles();
        await paginationState.SetTotalItemCountAsync(0);

        usersProvider = async request =>
        {
            isLoading = true;
    

            try
            {
                int page = paginationState.CurrentPageIndex + 1;
                int pageSize = paginationState.ItemsPerPage;

                string? sortBy = null;
                string? sortDirection = null;   
                
                if (request.SortByColumn != null)
                {
                   
                    var sortProperty = request.GetSortByProperties().FirstOrDefault();
            
                    if (!string.IsNullOrEmpty(sortProperty.PropertyName))
                    {
                        sortBy = sortProperty.PropertyName;
                        sortDirection = sortProperty.Direction == SortDirection.Ascending 
                            ? "asc" 
                            : "desc";
                    }
                }

                var result = await Mediator.Send(new GetAllUsersRequest(roleFilter, nameFilter, sortBy, sortDirection,page, pageSize));

                if (result.IsSuccess)
                {
                    await paginationState.SetTotalItemCountAsync(result.Value!.TotalCount);
                    return GridItemsProviderResult.From(result.Value.Items, result.Value.TotalCount);
                }
                else
                {
                    return GridItemsProviderResult.From(Array.Empty<UserDto>(), 0);
                }
            }
            catch (Exception ex)
            {
                _notification.ShowError("Error al cargar los usuarios.");
                return GridItemsProviderResult.From(Array.Empty<UserDto>(), 0);
            }
            finally
            {
                isLoading = false;
            }
        };
    }

    private void LoadRoles()
    {
        roleOptions = new List<Option<string>>
        {
            new() { Value = AppRoles.Administrador, Text = AppRoles.Administrador },
            new() { Value = AppRoles.JefeProceso, Text = AppRoles.JefeProceso },
            new() { Value = AppRoles.JefeArea, Text = AppRoles.JefeArea },
            new() { Value = AppRoles.UsuarioNormal, Text = AppRoles.UsuarioNormal }
        };
    }
    
    
    private async Task OnRoleFilterChanged(string selectedOption)
    {
        roleFilter = selectedOption;
        if (dataGrid != null)
        {
            await dataGrid.RefreshDataAsync(true);
        }
    }
    
    
    private async Task ViewUserDetails(UserDto user)
    {
        DialogParameters<UserDto> parameters = new()
        {
            Title = "Detalles del usuario",
            TitleTypo = Typography.H2,
            TrapFocus = false,
            PrimaryAction = null,
            SecondaryAction = null,
            Modal = true,
            PreventDismissOnOverlayClick = true,
            PreventScroll = true,
        };
        IDialogReference dialog = await DialogService.ShowDialogAsync<UserDetailsDialog>(user, parameters);
        DialogResult result = await dialog.Result;
        
        if (result.Data is not null)
        {
            UserDto? usuario = result.Data as UserDto;
            Console.WriteLine($"Dialog closed by {usuario.FullName} {usuario.Email} ({usuario.Id}) - Canceled: {result.Cancelled}");
        }
        else
        {
            Console.WriteLine($"Dialog closed - Canceled: {result.Cancelled}");
        }
    }

    private Appearance GetBadgeAppearance(string role)
    {
        return role switch
        {
            "Administrador" => Appearance.Accent,
            "JefeProceso" => Appearance.Lightweight,
            "JefeArea" => Appearance.Lightweight,
            _ => Appearance.Neutral
        };
    }
    
    private async Task ClearRoleFilterAsync()
    {
        roleFilter = null;
        
        if (dataGrid != null)
        {
            await dataGrid.RefreshDataAsync(true);
        }
    }
    
    private async Task ManageUserRole(UserDto user)
    {
        DialogParameters<ManageRoleDialog> parameters = new()
        {
            Title = null,
            TitleTypo = Typography.H2,
            TrapFocus = false, 
            ShowDismiss = false,
            ShowTitle = false,
            PrimaryAction = null,
            SecondaryAction = null,
            PreventDismissOnOverlayClick = true,
            PreventScroll = true,
        };

        IDialogReference dialog = await DialogService.ShowDialogAsync<ManageRoleDialog>(user,parameters);
        DialogResult result = await dialog.Result;

        if (!result.Cancelled)
        {
            await dataGrid.RefreshDataAsync(true);
        }
    }
    private async Task OnNameFilterChanged()
    {
        if (dataGrid != null)
        {
            await dataGrid.RefreshDataAsync(true);
        }
    }
    
    private async Task DeleteUserAsync(UserDto user)
    {
        var dialog = await DialogService.ShowConfirmationAsync(
            $"¿Eliminar al usuario {user.FullName} ({user.Email})?",
            "Confirmar eliminación",
            "Eliminar",
            "Cancelar");

        var dialogResult = await dialog.Result;
        
        if (dialogResult.Cancelled)
            return;

        var result = await Mediator.Send(new DeleteUserRequest(user.Id));
        
        if (result.IsSuccess)
        {
            _notification.ShowSuccess($"Usuario {user.FullName} eliminado correctamente.");
            await dataGrid.RefreshDataAsync(true);   
        }
        else
        {
            _notification.ShowError(string.Join(", ", result.Errors));
        }
    }
}