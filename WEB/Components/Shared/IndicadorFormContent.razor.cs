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
using WEB.Features.Indicador.Get;
using WEB.Features.Indicador.Update;
using WEB.Features.Objetivo.GetAll;
using WEB.Features.Proceso.Dto;
using WEB.Features.Proceso.Get;
using WEB.Features.Proceso.GetAll;

namespace WEB.Components.Shared;

public partial class IndicadorFormContent : ComponentBase
{
      [Parameter] public int? Id { get; set; }
      [Parameter] public EventCallback OnSaved { get; set; }
      [Parameter] public EventCallback OnCanceled { get; set; }
      [Parameter] public string? ReturnUrl { get; set; }
    
        private FluentWizard wizard = default!;
        private int currentStep = 0;
    
        private bool isLoading = true;
    
        private string nombre = string.Empty;
        private int procesoId;
        
        private string _metaCumplir = string.Empty;
        private string metaCumplir
        {
            get => _metaCumplir;
            set
            {
                _metaCumplir = value;
                if (MetaHelper.TryParsearMeta(value, out decimal val, out _))
                    _metaCumplirDecimal = val;
                else
                    _metaCumplirDecimal = 0;
            }
        }
        private decimal _metaCumplirDecimal;
        
        private IndicadorOrigen origen = IndicadorOrigen.MES;
        private IndicadorTipo tipo = IndicadorTipo.Escencial;
        private string? observacion;
        private string valorTotal = string.Empty;   
        private string valorReal = string.Empty;    
    
        private bool isJefeProceso = false;
        private int? fixedProcesoId = null;
        private string fixedProcesoNombre = "";
    
        private List<ProcesoDto> procesos = new();
        private List<ObjetivoSeleccionable> objetivos = new();
        private List<AreaDto> areas = new();
        
        private AreaTipo bulkTipo = AreaTipo.Facultad;
        private string bulkMeta = "";
        private HashSet<int> bulkExcluidos = new();
        private List<MetaPorAreaItem> bulkMetaItems = new();   
        private List<MetaPorAreaItem> manualMetaItems = new(); 
        
        private List<AreaDto> areasDelTipoSeleccionado => 
            areas.Where(a => a.Tipo == bulkTipo).ToList();
        
        private string? bulkTipoStr
        {
            get => ((int)bulkTipo).ToString();
            set
            {
                if (int.TryParse(value, out int val) && Enum.IsDefined(typeof(AreaTipo), val))
                    bulkTipo = (AreaTipo)val;
                else
                    bulkTipo = AreaTipo.Facultad; 
            }
        }
    
        private void ToggleBulkExclusion(int areaId, bool isChecked)
        {
            if (isChecked)
                bulkExcluidos.Add(areaId);
            else
                bulkExcluidos.Remove(areaId);
        }
        
        private async Task ApplyBulkAssignment()
        {
            if (string.IsNullOrWhiteSpace(bulkMeta)) return;
    
            if (!MetaHelper.TryParsearMeta(bulkMeta, out _, out bool isPorc) ||
                isPorc != IsMetaCumplirPorcentual)
            {
                _notificacion.ShowError(IsMetaCumplirPorcentual ?
                    "La meta debe ser un porcentaje (ej: 90%)" :
                    "La meta debe ser un número (ej: 90 o 20.5)");
                return;
            }
            
            var areasToAssign = areasDelTipoSeleccionado
                .Where(a => !bulkExcluidos.Contains(a.Id))
                .ToList();
            
            foreach (var area in areasToAssign)
            {
                if (manualMetaItems.Any(m => m.AreaId == area.Id))
                    continue;
    
                var existingBulk = bulkMetaItems.FirstOrDefault(b => b.AreaId == area.Id);
                if (existingBulk != null)
                    existingBulk.Meta = bulkMeta;
                else
                    bulkMetaItems.Add(new MetaPorAreaItem { AreaId = area.Id, Meta = bulkMeta });
            }
            
            var idsToKeep = areasToAssign.Select(a => a.Id).ToHashSet();
            var itemsToRemove = bulkMetaItems
                .Where(b => areas.Any(a => a.Id == b.AreaId && a.Tipo == bulkTipo) && !idsToKeep.Contains(b.AreaId))
                .ToList();
            foreach (var item in itemsToRemove)
                bulkMetaItems.Remove(item);
    
            bulkExcluidos.Clear();
            StateHasChanged();
        }
    
        private bool IsMetaCumplirPorcentual =>
            !string.IsNullOrEmpty(metaCumplir) &&
            MetaHelper.TryParsearMeta(metaCumplir, out _, out bool isPorc) &&
            isPorc;
    
        private string procesoIdStr
        {
            get => procesoId > 0 ? procesoId.ToString() : "";
            set => procesoId = int.TryParse(value, out var id) ? id : 0;
        }
    
        #region Validaciones
        private bool IsStep1Valid =>
            !string.IsNullOrWhiteSpace(nombre) &&
            procesoId > 0 &&
            !string.IsNullOrWhiteSpace(metaCumplir) &&
            !string.IsNullOrWhiteSpace(origen.GetDisplayName()) &&
            !string.IsNullOrWhiteSpace(tipo.GetDisplayName());
    
        private bool IsStep2Valid => IsStep1Valid && objetivos.Any(o => o.IsSelected);
    
        private bool IsStep3Valid =>
            IsStep2Valid &&
            bulkMetaItems.All(item => ValidarMetaItem(item)) &&
            manualMetaItems.All(item => ValidarMetaItem(item));
    
        private bool ValidarMetaItem(MetaPorAreaItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Meta)) return false;
            if (!MetaHelper.TryParsearMeta(item.Meta, out _, out bool isPorc)) return false;
            return isPorc == IsMetaCumplirPorcentual;
        }
        
        private bool IsCurrentStepValid(int stepIndex) => stepIndex switch
        {
            0 => IsStep1Valid,
            1 => IsStep2Valid,
            2 => IsStep3Valid,
            _ => true
        };
    
        private string ButtonText => Id == null ? "Crear Indicador" : "Guardar Cambios";
        #endregion
    
        protected override async Task OnInitializedAsync()
    {
        try
        {
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
                else throw new Exception("No tiene un proceso asignado.");
            }
            else
            {
                var procesoResult = await Mediator.Send(new GetAllProcesosRequest(Page: 1, PageSize: 50));
                if (procesoResult.IsSuccess)
                    procesos = procesoResult.Value.Items.ToList();
            }
            
            var areasResult = await Mediator.Send(new GetAllAreasRequest(Page: 1, PageSize: 100));
            if (areasResult.IsSuccess)
                areas = areasResult.Value.Items.ToList();
    
           
            var objResult = await Mediator.Send(new GetAllObjetivosRequest(Page: 1, PageSize: 100));
            if (objResult.IsSuccess)
                objetivos = objResult.Value.Items.Select(o => new ObjetivoSeleccionable
                {
                    Id = o.Id,
                    Nombre = o.Nombre,
                    NumeroObjetivo = o.NumeroObjetivo,
                    IsSelected = false
                }).ToList();
            
            if (Id.HasValue)
            {
                var indicadorResult = await Mediator.Send(new GetIndicadorByIdRequest(Id.Value));
                if (indicadorResult.IsSuccess && indicadorResult.Value is IndicadorDto dto)
                {
                    nombre = dto.Nombre;
                    procesoId = dto.Proceso.Id;
                    metaCumplir = dto.MetaCumplir;
                    origen = dto.Origen;
                    tipo = dto.Tipo;
                    observacion = dto.Observacion;
                    valorTotal = dto.ValorTotal ?? "";
                    valorReal = dto.ValorReal ?? "";
                    
                    foreach (var objDto in objetivos)
                        objDto.IsSelected = dto.Objetivos.Any(o => o.Id == objDto.Id);
                    
                    if (dto.Areas.Any())
                    {
                        var areasConTipo = dto.Areas
                            .Select(a => new { a.AreaId, a.MetaCumplir, Tipo = areas.FirstOrDefault(ar => ar.Id == a.AreaId)?.Tipo })
                            .Where(x => x.Tipo.HasValue)
                            .GroupBy(x => x.Tipo!.Value)
                            .ToList();
    
                        foreach (var grupo in areasConTipo)
                        {
                            var metaGroups = grupo.GroupBy(x => x.MetaCumplir)
                                .Select(g => new { Meta = g.Key, Count = g.Count(), Items = g.ToList() })
                                .OrderByDescending(g => g.Count)
                                .ToList();
                            
                            if (metaGroups.Count == 1)
                            {
                                foreach (var item in metaGroups[0].Items)
                                {
                                    bulkMetaItems.Add(new MetaPorAreaItem { AreaId = item.AreaId, Meta = item.MetaCumplir });
                                }
                            }
                            else if (metaGroups.Count > 1)
                            {
                              
                                var bulkGroup = metaGroups.First(); 
                                var manualGroups = metaGroups.Skip(1); 
    
                                foreach (var item in bulkGroup.Items)
                                {
                                    bulkMetaItems.Add(new MetaPorAreaItem { AreaId = item.AreaId, Meta = item.MetaCumplir });
                                }
    
                                foreach (var manualGroup in manualGroups)
                                {
                                    foreach (var item in manualGroup.Items)
                                    {
                                        manualMetaItems.Add(new MetaPorAreaItem { AreaId = item.AreaId, Meta = item.MetaCumplir });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar formulario: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }
    
    private async Task OnFinishAsync()
    {
        if (currentStep == 2)
        {
            var validation = ValidateStep3();
            if (validation.IsFailure)
            {
                ErrorNotification.ErrorToast(validation, _notificacion);
                return;
            }
        }

        var success = await Save();
        if (success)
            await OnSaved.InvokeAsync();
    }
    
        private async Task<bool> Save()
        {
            var objIds = objetivos.Where(o => o.IsSelected).Select(o => o.Id).ToList();
            if (!objIds.Any())
            {
                _notificacion.ShowError("Debe seleccionar al menos un objetivo.");
                return false;
            }
    
            Dictionary<int, string>? metaCumplirPorArea = null;
            var allItems = new Dictionary<int, string>();
    
            foreach (var bulk in bulkMetaItems)
                allItems[bulk.AreaId] = bulk.Meta;       
    
            foreach (var manual in manualMetaItems)
                allItems[manual.AreaId] = manual.Meta;   
    
            if (allItems.Any())
                metaCumplirPorArea = allItems;
    
            try
            {
                if (Id == null)
                {
                    var cmd = new CreateIndicadorCommand(
                        Nombre: nombre,
                        MetaCumplir: metaCumplir,
                        Origen: origen,
                        Tipo: tipo,
                        Observacion: observacion,
                        ProcesoId: procesoId,
                        ObjetivoIds: objIds,
                        ValorTotal: valorTotal,
                        ValorReal: valorReal,
                        MetaCumplirPorArea: metaCumplirPorArea
                    );
                    var result = await Mediator.Send(cmd);
                    if (result.IsSuccess) return true;
                    else LogError(result);
                }
                else
                {
                    var cmd = new UpdateIndicadorCommand(
                        Id: Id.Value,
                        Nombre: nombre,
                        MetaCumplir: metaCumplir,
                        Origen: origen,
                        Tipo: tipo,
                        Observacion: observacion,
                        ProcesoId: procesoId,
                        ObjetivoIds: objIds,
                        ValorTotal: valorTotal,
                        ValorReal: valorReal,
                        MetaCumplirPorArea: metaCumplirPorArea
                    );
                    var result = await Mediator.Send(cmd);
                    if (result.IsSuccess) return true;
                    else LogError(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
            return false;
        }
    
        private void LogError(Result result) =>
            _notificacion.ShowError(string.Join(", ", result.Errors.Select(e => e.Message)));
    
        private void AddAreaMeta()
        {
            var usedIds = bulkMetaItems.Select(b => b.AreaId)
                .Concat(manualMetaItems.Select(m => m.AreaId))
                .ToHashSet();
            var available = areas.FirstOrDefault(a => !usedIds.Contains(a.Id));
            if (available != null)
            {
                manualMetaItems.Add(new MetaPorAreaItem { AreaId = available.Id, Meta = "" });
                StateHasChanged();
            }
        }
    
        private void UpdateAreaId(int index, int newId)
        {
            if (index >= 0 && index < manualMetaItems.Count)
                manualMetaItems[index].AreaId = newId;
        }
        private void RemoveAreaMeta(int index) 
        {
            if (index >= 0 && index < manualMetaItems.Count)
                manualMetaItems.RemoveAt(index);
            StateHasChanged();
        }
        private void RemoveBulkItem(int index)
        {
            if (index >= 0 && index < bulkMetaItems.Count)
                bulkMetaItems.RemoveAt(index);
            StateHasChanged();
        }
        private void RemoveBulkItemByAreaId(int areaId)
        {
            var item = bulkMetaItems.FirstOrDefault(b => b.AreaId == areaId);
            if (item != null)
            {
                bulkMetaItems.Remove(item);
                StateHasChanged();
            }
        }
        private void RemoveManualItemByAreaId(int areaId)
        {
            var item = manualMetaItems.FirstOrDefault(m => m.AreaId == areaId);
            if (item != null)
            {
                manualMetaItems.Remove(item);
                StateHasChanged();
            }
        }
        private List<AreaDto> GetAvailableAreas(int currentAreaId)
        {
            var usedIds = bulkMetaItems.Select(b => b.AreaId)
                .Concat(manualMetaItems.Where(m => m.AreaId != currentAreaId).Select(m => m.AreaId))
                .ToHashSet();
            return areas.Where(a => !usedIds.Contains(a.Id) || a.Id == currentAreaId).ToList();
        }
    
        private async Task TryGoToNextStep()
        {
            if (currentStep == 0)
            {
                var validation = ValidateStep1();
                if (validation.IsFailure) { ErrorNotification.ErrorToast(validation, _notificacion); return; }
            }
            else if (currentStep == 1 && !IsStep2Valid)
            {
                _notificacion.ShowError("Debe seleccionar al menos un objetivo.");
                return;
            }
            else if (currentStep == 2)
            {
                var validation = ValidateStep3();
                if (validation.IsFailure) { ErrorNotification.ErrorToast(validation, _notificacion); return; }
            }
            await wizard.GoToStepAsync(currentStep + 1);
        }
    
        private Result<Unit> ValidateStep1()
        {
            var errors = new List<ErrorDetail>();
            if (string.IsNullOrWhiteSpace(nombre)) errors.Add(new ErrorDetail { Message = "Nombre requerido." });
            if (procesoId <= 0) errors.Add(new ErrorDetail { Message = "Seleccione un proceso." });
            if (!MetaHelper.TryParsearMeta(metaCumplir, out _, out _))
                errors.Add(new ErrorDetail { Message = "Meta a cumplir no válida." });
            return errors.Any() ? Result<Unit>.Fail(errors) : Result<Unit>.Success(Unit.Value);
        }
    
        private Result<Unit> ValidateStep3()
        {
            var errors = new List<ErrorDetail>();
    
            void ValidateItem(MetaPorAreaItem item, string tipo)
            {
                var areaName = areas.FirstOrDefault(a => a.Id == item.AreaId)?.Nombre ?? $"Área {item.AreaId}";
                if (string.IsNullOrWhiteSpace(item.Meta))
                {
                    errors.Add(new ErrorDetail { Message = $"{tipo} '{areaName}' sin meta asignada." });
                    return;
                }
                if (!MetaHelper.TryParsearMeta(item.Meta, out _, out bool isPorc))
                {
                    errors.Add(new ErrorDetail { Message = $"Meta inválida para {tipo} '{areaName}': {item.Meta}" });
                    return;
                }
                if (isPorc != IsMetaCumplirPorcentual)
                {
                    errors.Add(new ErrorDetail
                    {
                        Message = $"El área '{areaName}' debe tener un valor " +
                                  (IsMetaCumplirPorcentual ? "con %" : "sin %") +
                                  $" (actual: {item.Meta})."
                    });
                }
            }
            
            foreach (var item in bulkMetaItems)
            {
                var areaName = areas.FirstOrDefault(a => a.Id == item.AreaId)?.Nombre ?? $"Área {item.AreaId}";
                if (string.IsNullOrWhiteSpace(item.Meta))
                    errors.Add(new ErrorDetail { Message = $"Área masiva '{areaName}' sin meta asignada." });
                else if (!MetaHelper.TryParsearMeta(item.Meta, out _, out _))
                    errors.Add(new ErrorDetail { Message = $"Meta inválida para el área masiva '{areaName}': {item.Meta}" });
            }
            
            foreach (var item in manualMetaItems)
            {
                var areaName = areas.FirstOrDefault(a => a.Id == item.AreaId)?.Nombre ?? $"Área {item.AreaId}";
                if (string.IsNullOrWhiteSpace(item.Meta))
                    errors.Add(new ErrorDetail { Message = $"Área manual '{areaName}' sin meta asignada." });
                else if (!MetaHelper.TryParsearMeta(item.Meta, out _, out _))
                    errors.Add(new ErrorDetail { Message = $"Meta inválida para el área manual '{areaName}': {item.Meta}" });
            }
    
            return errors.Any() ? Result<Unit>.Fail(errors) : Result<Unit>.Success(Unit.Value);
        }
    
        private async Task Cancel()
        {
            await OnCanceled.InvokeAsync();
        }
    
        private void OnStepChange(FluentWizardStepChangeEventArgs e)
        {
            // para logging
        }
    
 
        public class ObjetivoSeleccionable
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
            public int NumeroObjetivo { get; set; }
            public bool IsSelected { get; set; }
        }
    
        public class MetaPorAreaItem
        {
            public int AreaId { get; set; }
            public string Meta { get; set; } = "";
        }
        
        private decimal SumaMetas =>
            bulkMetaItems.Sum(item => TryParseMeta(item.Meta)) +
            manualMetaItems.Sum(item => TryParseMeta(item.Meta));
    
        private decimal TryParseMeta(string? metaStr) =>
            MetaHelper.TryParsearMeta(metaStr, out decimal v, out _) ? v : 0;
    
        private string GetSumaColor()
        {
           
            if (IsMetaCumplirPorcentual || _metaCumplirDecimal <= 0)
                return "transparent";
    
            decimal pct = SumaMetas / _metaCumplirDecimal * 100;
    
            if (pct < 80) return "var(--error)";              
            if (pct < 100) return "#f97316";                    
            if (Math.Abs(pct - 100) < 0.001m) return "var(--success)"; 
            return "#3b82f6";                                   
        }
}