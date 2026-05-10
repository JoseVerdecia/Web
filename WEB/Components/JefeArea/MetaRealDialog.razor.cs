using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Globalization;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.IndicadorDeArea.Get;
using WEB.Features.IndicadorDeArea.Update;

namespace WEB.Components.JefeArea;

public partial class MetaRealDialog : ComponentBase, IDialogContentComponent
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    [Parameter] public object Content { get; set; } = default!;

    private bool _isPorcentual;
    private string _valorTotalLabel = "Valor Total";
    private string _valorRealLabel = "Valor Real";
    private decimal? _valorTotal;
    private decimal? _valorReal;
    private decimal? _metaRealNumerico;
    private string _error = string.Empty;
    private int _id;
    private string _valoracionCualitativa = string.Empty;

    protected override async Task OnParametersSetAsync ()
    {
      
            if (Content is IndicadorDeAreaDto dto)
            {
                _id = dto.Id;

                var result = await Mediator.Send(new GetIndicadorDeAreaRequest(_id));
                if (result.IsSuccess && result.Value is IndicadorDeAreaDto areaCompleta)
                {
                    _isPorcentual = areaCompleta.IsMetaCumplirPorcentual;

                    if (_isPorcentual)
                    {
                        _valorTotalLabel = /*areaCompleta.ValorTotalLabel ??*/ "Valor Total";
                        _valorRealLabel = /*areaCompleta.ValorRealLabel ??*/ "Valor Real";
                        _valorTotal = areaCompleta.ValorTotal;
                        _valorReal = areaCompleta.ValorReal;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(areaCompleta.MetaReal) &&
                            decimal.TryParse(areaCompleta.MetaReal, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                            _metaRealNumerico = val;
                        else
                            _metaRealNumerico = null;
                    }

                    _valoracionCualitativa = areaCompleta.ValorCualitativo ?? string.Empty;
                }
                else
                {
                    _error = "No se pudo cargar la información del indicador.";
                }
            }
        
    }

    private async Task Save()
    {
        _error = string.Empty;

        if (_isPorcentual)
        {
            if (_valorTotal == null || _valorReal == null)
            {
                _error = "Debe ingresar ambos valores (Valor Total y Valor Real).";
                return;
            }
        }
        else
        {
            if (_metaRealNumerico == null)
            {
                _error = "Debe ingresar un valor para la Meta Real.";
                return;
            }
        }

        var request = new UpdateMetaRealRequest(
            id: _id,
            metaReal: _isPorcentual ? null : _metaRealNumerico?.ToString(CultureInfo.InvariantCulture),
            ValorTotal: _isPorcentual ? _valorTotal : null,
            ValorReal: _isPorcentual ? _valorReal : null,
            ValoracionCualitativa: _valoracionCualitativa
        );

        var result = await Mediator.Send(request);
        if (result.IsSuccess && result.Value != null)
        {
            await Dialog.CloseAsync(result.Value);
        }
        else
        {
            _error = string.Join(", ", result.Errors.Select(e => e.Message));
        }
    }

    private async Task Cancel()
    {
        await Dialog.CancelAsync();
    }
}