using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Helpers;
using WEB.Core.Mediator;
using WEB.Enums;
using WEB.Features.Area.Dto;
using WEB.Features.Area.GetAll;
using WEB.Features.Indicador.Create;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.Update;
using WEB.Features.Objetivo.GetAll;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.Administrador.Indicador;

public partial class IndicadorFormDialog : ComponentBase, IDialogContentComponent
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;

 
    private string nombre = string.Empty;
    private int procesoId;
    private string metaCumplir = string.Empty;
    private string? metaReal;
    private IndicadorOrigen origen = IndicadorOrigen.MES;
    private IndicadorTipo tipo = IndicadorTipo.Escencial;
    private string? observacion;

    private List<MetaPorAreaItem> metasPorAreaList = new();


    private List<EnumCatalogItem> origenList = new();
    private List<EnumCatalogItem> tipoList = new();
    private List<ProcesoDto> procesos = new();
    private List<ObjetivoSeleccionable> objetivos = new();
    private List<AreaDto> areas = new();
    private List<int> objetivoIdsSelected = new();
    private bool isLoading = true;

    [Parameter] public object Content { get; set; } = default!;

    private bool IsValid =>
        !string.IsNullOrWhiteSpace(nombre) &&
        procesoId > 0 &&
        !string.IsNullOrWhiteSpace(metaCumplir) &&
        !string.IsNullOrWhiteSpace(origen.GetDisplayName()) &&
        !string.IsNullOrWhiteSpace(tipo.GetDisplayName()) &&
        objetivos.Any(o => o.IsSelected) &&
        (!metasPorAreaList.Any() || metasPorAreaList.All(m => !string.IsNullOrWhiteSpace(m.Meta)));

    protected override async Task OnInitializedAsync()
    {
        try
        {
            origenList = EnumHelper.GetCatalog<IndicadorOrigen>();
            tipoList = EnumHelper.GetCatalog<IndicadorTipo>();
            
            var procesoResult = await Mediator.Send(new GetAllProcesosRequest(Page: 1, PageSize: 20));
            procesos = procesoResult.Value.Items.ToList();
            
            var objetivoResult = await Mediator.Send(new GetAllObjetivosRequest(Page: 1, PageSize: 10));
            objetivos = objetivoResult.Value.Items.Select(o => new ObjetivoSeleccionable 
                { 
                    Id = o.Id, 
                    Nombre = o.Nombre, 
                    NumeroObjetivo = o.NumeroObjetivo,
                    IsSelected = false 
                })
                .ToList();

            var areasResult = await Mediator.Send(new GetAllAreasRequest(Page: 1, PageSize: 10));
            areas = areasResult.Value.Items.ToList();

            if (Content is UpdateIndicadorRequest updateRequest)
            {
                nombre = updateRequest.Nombre;
                procesoId = updateRequest.ProcesoId;
                metaCumplir = updateRequest.MetaCumplir;
                metaReal = updateRequest.MetaReal;
                origen = updateRequest.Origen;
                tipo = updateRequest.Tipo;
                observacion = updateRequest.Observacion;
                
                if (updateRequest.ObjetivoIds != null)
                {
                    foreach (var obj in objetivos)
                    {
                        if (updateRequest.ObjetivoIds.Contains(obj.Id))
                            obj.IsSelected = true;
                    }
                }

                if (updateRequest.MetaCumplirPorArea != null)
                {
                    metasPorAreaList = updateRequest.MetaCumplirPorArea
                        .Select(kvp => new MetaPorAreaItem { AreaId = kvp.Key, Meta = kvp.Value })
                        .ToList();
                }
            }
        }
        finally
        {
            isLoading = false;
        }
    }
    
    private string procesoIdStr
    {
        get => procesoId > 0 ? procesoId.ToString() : string.Empty;
        set
        {
            if (int.TryParse(value, out int id))
                procesoId = id;
            else
                procesoId = 0;
        }
    }
    
    

    private void AddAreaMeta()
    {
        var usedAreaIds = metasPorAreaList.Select(m => m.AreaId).ToHashSet();
        
        var availableArea = areas.FirstOrDefault(a => !usedAreaIds.Contains(a.Id));
    
        if (availableArea != null)
        {
            metasPorAreaList.Add(new MetaPorAreaItem 
            { 
                AreaId = availableArea.Id, 
                Meta = string.Empty 
            });
        }
        //  Si no hay áreas disponibles, no agregar nada (opcional: mostrar mensaje)
        
        StateHasChanged();
    }

    private void UpdateAreaId(int index, int newAreaId)
    {
        if (index >= 0 && index < metasPorAreaList.Count)
            metasPorAreaList[index].AreaId = newAreaId;
    }

    private void RemoveAreaMeta(int index)
    {
        if (index >= 0 && index < metasPorAreaList.Count)
            metasPorAreaList.RemoveAt(index);
        StateHasChanged();
    }

    private string TitleText =>
        Content switch
        {
            CreateIndicadorRequest => "Crear Indicador",
            UpdateIndicadorRequest => "Editar Indicador",
            _ => "Indicador"
        };

    private string ButtonText =>
        Content switch
        {
            CreateIndicadorRequest => "Crear",
            UpdateIndicadorRequest => "Editar",
            _ => "Indicador"
        };

    private async Task Save()
    {
        // Obtener IDs de objetivos seleccionados
        var objetivoIdsSelected = objetivos
            .Where(o => o.IsSelected)
            .Select(o => o.Id)
            .ToList();
        

        if (!objetivoIdsSelected.Any())
        {
            Console.WriteLine("ERROR: No hay objetivos seleccionados!");
            return;
        }

        
        Dictionary<int, string>? metaCumplirPorArea = null;
        if (metasPorAreaList.Any(m => !string.IsNullOrWhiteSpace(m.Meta)))
        {
            metaCumplirPorArea = metasPorAreaList
                .Where(m => !string.IsNullOrWhiteSpace(m.Meta))
                .ToDictionary(m => m.AreaId, m => m.Meta);
        }

        try
        {
            if (Content is CreateIndicadorRequest)
            {
                var request = new CreateIndicadorCommand(
                    Nombre: nombre,
                    MetaCumplir: metaCumplir,
                    MetaReal: metaReal,
                    Origen: origen,
                    Tipo: tipo,
                    Observacion: observacion,
                    ProcesoId: procesoId,
                    ObjetivoIds: objetivoIdsSelected,
                    MetaCumplirPorArea: metaCumplirPorArea
                    );

                var result = await Mediator.Send(request);

                if (result.IsSuccess && result.Value != null)
                {
                    await Dialog.CloseAsync(result.Value);
                }
                else
                {
                    ErrorNotification.ErrorToast(result, _notificacion);
                }
            }
            else if (Content is UpdateIndicadorRequest updateReq)
            {
                var request = new UpdateIndicadorCommand(
                    Id: updateReq.Id,
                    Nombre: nombre,
                    MetaCumplir: metaCumplir,
                    MetaReal: metaReal,
                    Origen: origen,
                    Tipo: tipo,
                    Observacion: observacion,
                    ProcesoId: procesoId,
                    ObjetivoIds: objetivoIdsSelected.Any() ? objetivoIdsSelected : null,
                    MetaCumplirPorArea: metaCumplirPorArea
                );

                var updatedIndicador = await Mediator.Send(request);

                if (updatedIndicador != null)
                {
                    await Dialog.CloseAsync(updatedIndicador);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in Save: {ex.Message}");
        }
    }
    
    private void OnObjetivoChecked(int index, bool? isChecked)
    {
        if (index >= 0 && index < objetivos.Count)
        {
            objetivos[index].IsSelected = isChecked == true;
        }
    }
    private async Task Cancel()
    {
        await Dialog.CancelAsync();
    }
    
    private List<AreaDto> GetAvailableAreas(int currentAreaId)
    {
        var usedAreaIds = metasPorAreaList
            .Select(m => m.AreaId)
            .Where(id => id != currentAreaId)  
            .ToHashSet();

        return areas
            .Where(a => !usedAreaIds.Contains(a.Id))
            .ToList();
    }
}