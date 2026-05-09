using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Helpers;
using WEB.Features.Proceso.Create;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.Update;

namespace WEB.Components.Administrador.Proceso;

public partial class ProcesoFormDialog : ComponentBase, IDialogContentComponent
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    private string nombre = string.Empty;
    private bool IsValid => !string.IsNullOrWhiteSpace(nombre);
    [Parameter] public object Content { get; set; } = default!;
    
    
    protected override async Task OnInitializedAsync()
    {
        if (Content is CreateProcesoRequest)
        {
            // Modo creación: no hacer nada
        }
        else if (Content is UpdateProcesoRequest updateRequest)
        {
            nombre = updateRequest.Nombre;
        }
        
    }

    private string TitleText =>
        Content switch
        {
            CreateProcesoRequest => "Crear Proceso",
            UpdateProcesoRequest => "Editar Proceso",
            _ => "Proceso"
        };
    
    private string ButtonText =>
        Content switch
        {
            CreateProcesoRequest => "Crear",
            UpdateProcesoRequest => "Editar",
            _ => "Proceso"
        };

    private async Task Save()
    {
        if (Content is CreateProcesoRequest)
        {
            var request = new CreateProcesoCommand( Nombre : nombre);
            var result = await Mediator.Send(request);
            
            /*var result = await ProcesoService.CreateAsync(request);*/
            
            if (result.IsSuccess && result.Value != null)
                await Dialog.CloseAsync(result.Value);
            else
                ErrorNotification.ErrorToast(result,NotificacionService);
        }
        else if (Content is UpdateProcesoRequest updateReq)
        {
            var request = new UpdateProcesoCommand(Id: updateReq.Id, Nombre: nombre);
           
            var result = await Mediator.Send(request);
            
            if (result.Value != null)
                await Dialog.CloseAsync(result.Value);
        }
       
    }

    private async Task Cancel()
    {
        await Dialog.CancelAsync();
    }
}