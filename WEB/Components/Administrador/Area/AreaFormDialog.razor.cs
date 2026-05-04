using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Helpers;
using WEB.Enums;
using WEB.Features.Area.Create;
using WEB.Features.Area.Dto;
using WEB.Features.Area.Update;

namespace WEB.Components.Administrador.Area;

public partial class AreaFormDialog : ComponentBase, IDialogContentComponent
{
      [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    
        private string nombre = string.Empty;
        private AreaTipo tipoArea = AreaTipo.Facultad;
        [Parameter] public object Content { get; set; } = default!;
        
        
        protected override async Task OnInitializedAsync()
        {
            if (Content is CreateAreaRequest)
            {
                // Modo creación
            }
            else if (Content is UpdateAreaRequest updateRequest)
            {
                nombre = updateRequest.Nombre;
                tipoArea = updateRequest.Tipo;
            }
            
    
            // Aqui si quiero asigno un valor al Tipo de Area , pero lo dejo vacio para que el usuario seleccione
        }
    
        private string TitleText =>
            Content switch
            {
                CreateAreaRequest => "Crear Área",
                UpdateAreaRequest => "Editar Área",
                _ => "Área"
            };
        
        private string ButtonText =>
            Content switch
            {
                CreateAreaRequest => "Crear",
                UpdateAreaRequest => "Editar",
                _ => "Área"
            };
    
        private async Task Save()
        {
            if (Content is CreateAreaRequest)
            {
                var request = new CreateAreaCommand(Nombre : nombre, Tipo : tipoArea );
                var result = await Mediator.Send(request);
                
                if (result.IsSuccess && result.Value != null)
                {
                    await Dialog.CloseAsync(result.Value);
                }
                else
                {
                    ErrorNotification.ErrorToast(result,_notificacion);
                }
            }
            else if (Content is UpdateAreaRequest updateReq)
            {
                var request = new UpdateAreaCommand
                (
                    Id : updateReq.Id,
                    Nombre : nombre,
                    Tipo : tipoArea
                );
                var updatedArea = await Mediator.Send(request);
                if (updatedArea.IsSuccess)
                {
                    await Dialog.CloseAsync(updatedArea.Value);
                }
            }
           
        }
    
        private async Task Cancel()
        {
            await Dialog.CancelAsync();
        }
}