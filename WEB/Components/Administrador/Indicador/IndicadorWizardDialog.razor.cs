using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WEB.Common;
using WEB.Core.Extensions;
using WEB.Core.Helpers;
using WEB.Core.Result;
using WEB.Data;
using WEB.Enums;
using WEB.Features.Area.Dto;
using WEB.Features.Area.GetAll;
using WEB.Features.Indicador.Create;
using WEB.Features.Indicador.Dto;
using WEB.Features.Indicador.Update;
using WEB.Features.Objetivo.Dto;
using WEB.Features.Objetivo.GetAll;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.Get;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.Administrador.Indicador;

public partial class IndicadorWizardDialog : ComponentBase, IDialogContentComponent
{
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
  
    private FluentWizard wizard = default!;
    private int currentStep = 0;

    private bool isMobile;
    private bool isSmallScreen;
    
    private void HandleBreakpoint(GridItemSize size)
    {
        isSmallScreen = size == GridItemSize.Xs || size == GridItemSize.Sm;
        StateHasChanged();
    }
    
    private string nombre = string.Empty;
    private int procesoId;
    private string metaCumplir = string.Empty;
    private IndicadorOrigen origen = IndicadorOrigen.MES;
    private IndicadorTipo tipo = IndicadorTipo.Escencial;
    private string? observacion;
    private string valorTotal = string.Empty;
    private string valorReal = string.Empty;
    private bool isJefeProceso = false;
    private int? fixedProcesoId = null;
    private string fixedProcesoNombre = "";
    
    private List<MetaPorAreaItem> metasPorAreaList = new();
    private List<EnumCatalogItem> origenList = new();
    private List<EnumCatalogItem> tipoList = new();
    private List<ProcesoDto> procesos;
    private List<ObjetivoSeleccionable> objetivos;
    private List<AreaDto> areas;
    private bool isLoading = true;

    [Parameter] public object Content { get; set; } = default!;

    #region Validaciones por paso

    /// <summary>
    /// Validación del Paso 1: Información General (incluye clasificación)
    /// </summary>
    private bool IsStep1Valid =>
        !string.IsNullOrWhiteSpace(nombre) &&
        procesoId > 0 &&
        !string.IsNullOrWhiteSpace(metaCumplir) &&
        !string.IsNullOrWhiteSpace(origen.GetDisplayName()) &&
        !string.IsNullOrWhiteSpace(tipo.GetDisplayName());
    
    private bool IsMetaCumplirPorcentual =>
        !string.IsNullOrEmpty(metaCumplir) &&
        MetaHelper.TryParsearMeta(metaCumplir, out _, out bool isPorc) &&
        isPorc;

    /// <summary>
    /// Validación del Paso 2: Objetivos (al menos uno seleccionado)
    /// </summary>
    private bool IsStep2Valid =>
        IsStep1Valid &&
        objetivos.Any(o => o.IsSelected);

    /// <summary>
    /// Validación del Paso 3: Metas por Área (opcional, pero si hay items deben tener meta)
    /// </summary>
    private bool IsStep3Valid =>
        IsStep2Valid &&
        (!metasPorAreaList.Any() || metasPorAreaList.All(m =>
            !string.IsNullOrWhiteSpace(m.Meta) &&
            MetaHelper.TryParsearMeta(m.Meta, out _, out _)));

    /// <summary>
    /// Valida si el paso actual es válido para avanzar
    /// </summary>
    private bool IsCurrentStepValid(int stepIndex)
    {
        return stepIndex switch
        {
            0 => IsStep1Valid,
            1 => IsStep2Valid,
            2 => IsStep3Valid,
            _ => true
        };
    }

    /// <summary>
    /// Validación general del formulario completo
    /// </summary>
    private bool IsValid =>
        !string.IsNullOrWhiteSpace(nombre) &&
        procesoId > 0 &&
        !string.IsNullOrWhiteSpace(metaCumplir) &&
        !string.IsNullOrWhiteSpace(origen.GetDisplayName()) &&
        !string.IsNullOrWhiteSpace(tipo.GetDisplayName()) &&
        objetivos.Any(o => o.IsSelected) &&
        (!metasPorAreaList.Any() || metasPorAreaList.All(m => !string.IsNullOrWhiteSpace(m.Meta)));

    #endregion

   
   protected override async Task OnInitializedAsync()
{
    try
    {
        origenList = EnumHelper.GetCatalog<IndicadorOrigen>();
        tipoList = EnumHelper.GetCatalog<IndicadorTipo>();

        isJefeProceso = CurrentUser.IsInRole(AppRoles.JefeProceso);
        var user = await CurrentUser.GetUserAsync();

        if (isJefeProceso)
        {
            var procesoResult = await Mediator.Send(new GetProcesoByJefeIdRequest(user.Id));
            
            if (procesoResult.IsSuccess && procesoResult.Value is not null)
            {
                fixedProcesoId = procesoResult.Value.Id;
                fixedProcesoNombre = procesoResult.Value.Nombre;
                procesoId = fixedProcesoId.Value;
            }
            else
            {
                throw new Exception("No tiene un proceso asignado.");
            }
        }
        else
        {
            var procesoResult = await Mediator.Send(new GetAllProcesosRequest(Page: 1, PageSize: 50));
            procesos = procesoResult.Value.Items.ToList();
            
            if (isJefeProceso && fixedProcesoId.HasValue)
            {
                procesoId = fixedProcesoId.Value;
            }
        }
        
        var areasResult = await Mediator.Send(new GetAllAreasRequest(Page: 1, PageSize: 100));
        areas = areasResult.Value.Items.ToList();

        Result<PagedResult<ObjetivoDto>> objetivoResult = await Mediator.Send(new GetAllObjetivosRequest(Page: 1, PageSize: 100));
        
        objetivos = objetivoResult.Value.Items.Select(o => new ObjetivoSeleccionable
        {
            Id = o.Id,
            Nombre = o.Nombre,
            NumeroObjetivo = o.NumeroObjetivo,
            IsSelected = false
        }).ToList();
        
        if (Content is UpdateIndicadorRequest updateRequest)
        {
            nombre = updateRequest.Nombre;
            procesoId = updateRequest.ProcesoId;
            metaCumplir = updateRequest.MetaCumplir;
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
    catch (Exception ex)
    {
        Console.WriteLine($"Error al cargar datos iniciales: {ex.Message}");
        procesos ??= new List<ProcesoDto>();
        objetivos = new List<ObjetivoSeleccionable>();
        areas = new List<AreaDto>();
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
    
    private void OnStepChange(FluentWizardStepChangeEventArgs e)
    {
        Console.WriteLine($"Cambiando al paso {e.TargetLabel} (#{e.TargetIndex})");
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
            CreateIndicadorRequest => "Crear Indicador",
            UpdateIndicadorRequest => "Guardar Cambios",
            _ => "Guardar"
        };

    private async Task OnFinishAsync()
    {
        await Save();
    }

    private async Task Save()
    {
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
                var request = new CreateIndicadorCommand
                (
                    Nombre : nombre,
                    MetaCumplir : metaCumplir,
                    Origen :origen,
                    Tipo : tipo,
                    Observacion: observacion,
                    ProcesoId : procesoId,
                    ObjetivoIds : objetivoIdsSelected,
                    ValorTotal:valorTotal,
                    ValorReal:valorReal,
                    MetaCumplirPorArea : metaCumplirPorArea
                );

                var result = await Mediator.Send(request);

                if (result.IsSuccess  && result.Value != null)
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
                var request = new UpdateIndicadorCommand    
                (
                    Id : updateReq.Id,
                    Nombre : nombre,
                    MetaCumplir: metaCumplir,
                    Origen : origen,
                    Tipo : tipo,
                    Observacion : observacion,
                    ProcesoId : procesoId,
                    ObjetivoIds : objetivoIdsSelected.Any() ? objetivoIdsSelected : null,
                    ValorTotal:valorTotal,
                    ValorReal:valorReal,
                    MetaCumplirPorArea : metaCumplirPorArea
                );

                var result = await Mediator.Send(request);

                if (result.IsSuccess)
                {
                    await Dialog.CloseAsync(result.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in Save: {ex.Message}");
        }
    }

    private void OnObjetivoChecked(int index, bool isChecked)
    {
        if (index >= 0 && index < objetivos.Count)
        {
            objetivos[index].IsSelected = isChecked;
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
    
    private async Task TryGoToNextStep()
    {
        if (currentStep == 0)
        {
            var validationResult = ValidateStep1();
            if (validationResult.IsFailure)
            {
                ErrorNotification.ErrorToast(validationResult, _notificacion);
                return;
            }
        }
        else if (currentStep == 1)
        {
            if (!IsStep2Valid)
            {
                _notificacion.ShowError("Debe seleccionar al menos un objetivo.");
                return;
            }
        }
        else if (currentStep == 2) 
        {
            var validationResult = ValidateStep3();
            if (validationResult.IsFailure)
            {
                ErrorNotification.ErrorToast(validationResult, _notificacion);
                return;
            }
        }

        await wizard.GoToStepAsync(currentStep + 1);
    }

    private bool IsMetaCumplirValid =>
        !string.IsNullOrWhiteSpace(metaCumplir) &&
        MetaHelper.TryParsearMeta(metaCumplir, out _, out _);
    
    private Result<Unit> ValidateStep1()
    {
        var errors = new List<ErrorDetail>();

        if (string.IsNullOrWhiteSpace(nombre))
            errors.Add(new ErrorDetail { Message = "El nombre del indicador es requerido." });

        if (procesoId <= 0)
            errors.Add(new ErrorDetail { Message = "Debe seleccionar un proceso." });

        if (!IsMetaCumplirValid)
            errors.Add(new ErrorDetail { Message = "La meta a cumplir no tiene un formato válido (ej: 90, 90.5%)." });
        
        return errors.Any() ? Result<Unit>.Fail(errors) : Result<Unit>.Success(Unit.Value);
    }
    
    private Result<Unit> ValidateStep3()
    {
        var errors = new List<ErrorDetail>();

        if (!metasPorAreaList.Any())
            return Result<Unit>.Success(Unit.Value);

        foreach (var item in metasPorAreaList)
        {
            var area = areas.FirstOrDefault(a => a.Id == item.AreaId);
            var areaName = area?.Nombre ?? $"Área ID {item.AreaId}";

            if (string.IsNullOrWhiteSpace(item.Meta))
            {
                errors.Add(new ErrorDetail
                {
                    Message = $"El área '{areaName}' no tiene una meta asignada."
                });
            }
            else if (!MetaHelper.TryParsearMeta(item.Meta, out _, out _))
            {
                errors.Add(new ErrorDetail
                {
                    Message = $"La meta '{item.Meta}' para el área '{areaName}' no tiene un formato válido (ej: 90, 90.5%)."
                });
            }
        }

        return errors.Any() ? Result<Unit>.Fail(errors) : Result<Unit>.Success(Unit.Value);
    }
}