using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Features.Indicador.Dto;

namespace WEB.Common;

/// <summary>
/// GridItem jerárquico para FluentDataGrid.
/// Permite mostrar los IndicadoresDeArea como hijos expandibles de cada Indicador.
/// Hereda de HierarchicalGridItem para soportar expansión/colapso.
/// </summary>
public class IndicadorGridItem : HierarchicalGridItem<IndicadorDisplayItem, IndicadorGridItem>
{
    /// <summary>
    /// Convierte una lista de IndicadorDto en IndicadorGridItem con sus (IndicadoresDeArea).
    /// </summary>
    /// <param name="indicadores">Lista de indicadores a convertir</param>
    /// <param name="startCollapsed">Si true, los hijos se muestran colapsados inicialmente</param>
    /// <returns>Lista de IndicadorGridItem listos para usar en FluentDataGrid</returns>
    public static List<IndicadorGridItem> CreateFromIndicadores(List<IndicadorDto> indicadores, bool startCollapsed = true)
    {
        var items = new List<IndicadorGridItem>();

        foreach (var indicador in indicadores)
        {
            var parentItem = new IndicadorGridItem
            {
                Item = IndicadorDisplayItem.FromIndicadorDto(indicador),
                IsCollapsed = startCollapsed
            };
            
            items.Add(parentItem);
            
            if (indicador.Areas.Any())
            {
                foreach (var area in indicador.Areas)
                {
                    var childItem = new IndicadorGridItem
                    {
                        Item = IndicadorDisplayItem.FromIndicadorDeAreaDto(area, indicador.Id),
                        Depth = 1,
                        IsHidden = startCollapsed
                    };
                    parentItem.Children.Add(childItem);
                    items.Add(childItem);
                }
            }
        }

        return items;
    }

   
    public void ToggleExpanded()
    {
        IsCollapsed = !IsCollapsed;
        
        foreach (var child in Children)
        {
            child.IsHidden = IsCollapsed;
        }
    }
}