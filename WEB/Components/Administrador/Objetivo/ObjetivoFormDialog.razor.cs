using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Helpers;
using WEB.Features.Objetivo.Create;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.Update;

namespace WEB.Components.Administrador.Objetivo;

public partial class ObjetivoFormDialog : ComponentBase
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

    private string nombre = string.Empty;
    private int numeroObjetivo;
    private bool IsValid => !string.IsNullOrWhiteSpace(nombre);
    [Parameter] public object Content { get; set; } = default!;
    
    
    protected override async Task OnInitializedAsync()
    {
        if (Content is CreateObjetivoRequest)
        {
            numeroObjetivo = 1;
        }
        else if (Content is UpdateObjetivoRequest updateRequest)
        {
            nombre = updateRequest.Nombre;
            numeroObjetivo = updateRequest.NumeroObjetivo;
        }
        
    }

    private string TitleText =>
        Content switch
        {
            CreateObjetivoRequest => "Crear Objetivo",
            UpdateObjetivoRequest => "Editar Objetivo",
            _ => "Objetivo"
        };
    
    private string ButtonText =>
        Content switch
        {
            CreateObjetivoRequest => "Crear",
            UpdateObjetivoRequest => "Editar",
            _ => "Objetivo"
        };

    private async Task Save()
    {
        if (Content is CreateObjetivoRequest)
        {
            var request = new CreateObjetivoCommand (Nombre : nombre, NumeroObjetivo:numeroObjetivo);
            var result = await Mediator.Send(request);
           
            if (result.IsFailure)
            {
                ErrorNotification.ErrorToast(result, _notificacion);
                return;
            }
            
            if (result.IsSuccess && result.Value != null)
            {
                await Dialog.CloseAsync(result.Value);
            }
            else
            {
                ErrorNotification.ErrorToast(result,_notificacion);
            }
        }
        else if (Content is UpdateObjetivoRequest updateReq)
        {
            var request = new UpdateObjetivoCommand(Id : updateReq.Id, Nombre : nombre , NumeroObjetivo:numeroObjetivo);
            
            var result = await Mediator.Send(request);
            
            if (result.IsSuccess)
            {
                await Dialog.CloseAsync(result.Value);
            }
        }
       
    }

    private async Task Cancel()
    {
        await Dialog.CancelAsync();
    }
}