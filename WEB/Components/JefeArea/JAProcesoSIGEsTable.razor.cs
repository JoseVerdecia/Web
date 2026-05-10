using Microsoft.AspNetCore.Components;
using WEB.Common;
using WEB.Enums;
using WEB.Features.Indicador.GetAll;
using WEB.Features.IndicadorDeArea.GetAll;

namespace WEB.Components.JefeArea;

public partial class JAProcesoSIGEsTable : ComponentBase
{
      [Parameter] public int ProcesoId { get; set; }
    [Parameter] public int AreaId { get; set; }

    private List<IndicadorDeAreaExtendido> items = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        isLoading = true;
        try
        {
            var resultProc = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(ProcesoId));
            if (!resultProc.IsSuccess || resultProc.Value == null) return;

            var indicadoresProc = resultProc.Value.ToList();
            if (!indicadoresProc.Any()) return;

            var resultArea = await Mediator.Send(new GetAllIndicadoresDeAreaByAreaRequest(AreaId));
            if (!resultArea.IsSuccess || resultArea.Value == null) return;

            var indicadoresArea = resultArea.Value.ToList();

            items = indicadoresProc.Select(ind =>
            {
                var area = indicadoresArea.FirstOrDefault(ia => ia.IndicadorPadreId == ind.Id);
                if (area == null) return null;

                return new IndicadorDeAreaExtendido
                {
                    IndicadorPadreId = ind.Id,
                    NombreIndicadorPadre = ind.Nombre,
                    Objetivos = ind.Objetivos,
                    EsEsencial = ind.Tipo == IndicadorTipo.Escencial,
                    MetaCumplir = area.MetaCumplir,
                    MetaReal = area.MetaReal,
                    MetaCumplirDecimal = area.MetaCumplirDecimal,
                    MetaRealDecimal = area.MetaRealDecimal,
                    IsMetaCumplirPorcentual = area.IsMetaCumplirPorcentual,
                    IsRealPorcentual = area.IsRealPorcentual,
                    ValorTotal = area.ValorTotal,
                    ValorReal = area.ValorReal,
                    ValorTotalLabel = /*area.ValorTotalLabel ??*/ "Valor Total",
                    ValorRealLabel = /*area.ValorRealLabel ??*/ "Valor Real",
                    Evaluacion = area.Evaluacion
                };
            }).Where(x => x != null).ToList();
        }
        catch
        {
            items = new();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

   
}