using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Core.Mediator;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.IndicadorDeArea.Update;

namespace WEB.Components.JefeArea.Indicador;

public partial class MetaRealDialog : ComponentBase, IDialogContentComponent
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    
    // ✅ USA OBJECT, como tus otros diálogos
    [Parameter] public object Content { get; set; } = default!;
    
    private string _metaReal = string.Empty;
    private string _error = string.Empty;
    private int _id;

    protected override void OnInitialized()
    {
        // ✅ Casteo manual, como tus otros diálogos
        if (Content is IndicadorDeAreaDto dto)
        {
            _id = dto.Id;
            _metaReal = dto.MetaReal ?? "";
        }
    }

    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(_metaReal))
        {
            _error = "Debe ingresar un valor";
            return;
        }

        var request = new UpdateMetaRealRequest(_id, _metaReal);
        var result = await Mediator.Send(request);
        
        if (result.IsSuccess && result.Value != null)
        {
            await Dialog.CloseAsync(result.Value);
        }
    }

    private async Task Cancel()
    {
        await Dialog.CancelAsync();
    }
}