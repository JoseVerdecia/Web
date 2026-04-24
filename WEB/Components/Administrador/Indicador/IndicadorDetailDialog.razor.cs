using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Indicador.Dto;

namespace WEB.Components.Administrador.Indicador;

public partial class IndicadorDetailDialog : ComponentBase, IDialogContentComponent
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    [Parameter] public IndicadorDto Content { get; set; } = default!;

    private async Task Close()
    {
        await Dialog.CloseAsync();
    }
}