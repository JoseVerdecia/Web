using Microsoft.AspNetCore.Components;
using WEB.Features.Indicador.GetAll;
using WEB.Features.IndicadorDeArea.Dto;
using WEB.Features.Objetivo.Dto;

namespace WEB.Components.JefeArea;

public partial class PEJAProcesoTable : ComponentBase
{
   [Parameter] public int ProcesoId { get; set; }
      [Parameter] public int AreaId { get; set; }
  
      private List<ItemTabla> items = new();
      private bool isLoading = true;
  
      protected override async Task OnInitializedAsync()
      {
          await CargarDatos();
      }
  
      protected override async Task OnParametersSetAsync()
      {
          await CargarDatos();
      }
  
      private async Task CargarDatos()
      {
          isLoading = true;
          StateHasChanged();
          try
          {
              var result = await Mediator.Send(new GetAllIndicadoresByProcesoRequest(ProcesoId));
              if (result.IsSuccess && result.Value != null)
              {
                 
                  var indicadoresDelProceso = result.Value.ToList();
                  items = indicadoresDelProceso
                      .SelectMany(ind => ind.Areas ?? Enumerable.Empty<IndicadorDeAreaDto>())
                      .Where(area => area.AreaId == AreaId)
                      .Select(area => new ItemTabla
                      {
                          NombreIndicador = area.NombreIndicadorPadre,
                          Objetivos = indicadoresDelProceso.FirstOrDefault(ind => ind.Id == area.IndicadorPadreId)?.Objetivos ?? new List<ObjetivoSimpleDto>(),
                          MetaCumplirArea = area.MetaCumplir,
                          /*EsEsencial = ind.Tipo == IndicadorTipo.Escencial */
                      })
                      .ToList();
              }
          }
          catch
          {
              items = new();
          }
          finally
          {
              isLoading = false;
              await InvokeAsync(StateHasChanged);
          }
      }
  
      private class ItemTabla
      {
          public string NombreIndicador { get; set; } = "";
          public List<ObjetivoSimpleDto> Objetivos { get; set; } = new();
          public string MetaCumplirArea { get; set; } = "";
          public bool EsEsencial { get; set; }
      }
}