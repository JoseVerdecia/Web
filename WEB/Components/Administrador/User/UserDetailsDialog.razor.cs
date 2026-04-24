using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Services;
using WEB.Data;
using WEB.Features.Area.Denegar;
using WEB.Features.Proceso.Denegar;
using WEB.Features.Users.Dto;

namespace WEB.Components.Administrador.User;

public partial class UserDetailsDialog : ComponentBase, IDialogContentComponent
{
      [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    [Parameter] public UserDto? Content { get; set; }
    
    private bool isRemoving = false;

    private Appearance GetBadgeAppearance(string role)
    {
        return role switch
        {
            AppRoles.Administrador => Appearance.Accent,
            AppRoles.JefeProceso => Appearance.Accent,
            AppRoles.JefeArea => Appearance.Lightweight,
            _ => Appearance.Neutral
        };
    }

    private async Task OpenAssignRoleDialog()
    {
        if (Content == null) return;

        var parameters = new DialogParameters<UserDto>
        {
            Title = $"Asignar rol a {Content.FullName}",
            TitleTypo = Typography.H2,
            TrapFocus = false,
            PreventDismissOnOverlayClick = true,
            PreventScroll = true,
        };

        var dialog = await DialogService.ShowDialogAsync<ManageRoleDialog>(Content, parameters);
        var result = await dialog.Result;
        
        if (!result.Cancelled && result.Data is not null)
        {
            await Dialog.CloseAsync(DialogResult.Ok(Content));
        }
    }

    private async Task RemoveRole()
    {
        if (Content == null) return;

        isRemoving = true;
        StateHasChanged();

        try
        {
            if (Content.Role == AppRoles.JefeArea && Content.AreaId.HasValue)
            {
                var result = await Mediator.Send(new DenegarResponsableAreaRequest(Content.Id,Content.AreaId));
                if (result.IsSuccess)
                {
                    NotificationService.ShowSuccess($"Se quitó el rol de Jefe de Área a {Content.FullName}");
                    await Dialog.CloseAsync(DialogResult.Ok(Content));
                }
                else
                {
                    NotificationService.ShowError("Error al quitar la asignación");
                }
            }
            else if (Content.Role == AppRoles.JefeProceso && Content.ProcesoId.HasValue)
            {
                var result = await Mediator.Send(new DenegarResponsableRequest(Content.Id, Content.ProcesoId));
                if (result is { IsSuccess: true })
                {
                    NotificationService.ShowSuccess($"Se quitó el rol de Jefe de Proceso a {Content.FullName}");
                    await Dialog.CloseAsync(DialogResult.Ok(Content));
                }
                else
                {
                    NotificationService.ShowError("Error al quitar la asignación");
                }
            }
        }
        catch (Exception)
        {
            NotificationService.ShowError("Error de conexión con el servidor");
        }
        finally
        {
            isRemoving = false;
        }
    }

    private async Task Close()
    {
        await Dialog.CloseAsync();
    }
}