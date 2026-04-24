using WEB.Core.Extensions;
using WEB.Enums;
using WEB.Features.Indicador.Dto;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Common;

public class IndicadorDisplayItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string MetaCumplir { get; set; } = string.Empty;
    public string? MetaReal { get; set; }
    public Evaluacion Evaluacion { get; set; }
    public string EvaluacionColor { get; set; } = string.Empty;
    
    public string? Origen { get; set; }
    public string? Tipo { get; set; }
    public bool IsEscencial { get; set; }
    public string? Observacion { get; set; }
    public string? ProcesoNombre { get; set; }
    public DateTime? DeleteAt { get; set; }
    public bool IsSelected { get; set; } 
    public string? ObjetivosFormatted { get; set; }
    
    public int CantidadAreas { get; set; }
    
    public int? AreaId { get; set; }
    public string? AreaNombre { get; set; }
    public int IndicadorPadreId { get; set; }
    public bool IsParent { get; set; }
    public List<IndicadorDeAreaDto> Areas { get; set; } = new();
    
    public static IndicadorDisplayItem FromIndicadorDto(IndicadorDto dto)
    {
        return new IndicadorDisplayItem
        {
            Id = dto.Id,
            Nombre = dto.Nombre,
            MetaCumplir = dto.MetaCumplir,
            MetaReal = dto.MetaReal,
            Evaluacion = dto.Evaluacion,
            EvaluacionColor = dto.EvaluacionColor,
            Origen = dto.Origen.GetDisplayName(),
            Tipo = dto.Tipo.GetDisplayName(),
            IsEscencial = dto.IsTipoEscencial,
            Observacion = dto.Observacion,
            ProcesoNombre = dto.Proceso?.Nombre,
            DeleteAt = dto.DeleteAt,
            ObjetivosFormatted = dto.Objetivos.Any()
                ? string.Join(" | ", dto.Objetivos.Select(o => o.NumeroObjetivo))
                : "Sin objetivos",
            CantidadAreas = dto.Areas?.Count ?? 0,
            IsParent = true,
            Areas = dto.Areas ?? new List<IndicadorDeAreaDto>()
        };
    }
    
    public static IndicadorDisplayItem FromIndicadorDeAreaDto(IndicadorDeAreaDto dto, int indicadorPadreId)
    {
        return new IndicadorDisplayItem
        {
            Id = dto.AreaId,
            Nombre = dto.AreaNombre,
            MetaCumplir = dto.MetaCumplir,
            MetaReal = dto.MetaReal,
            Evaluacion = dto.Evaluacion,
            AreaId = dto.AreaId,
            AreaNombre = dto.AreaNombre,
            IndicadorPadreId = indicadorPadreId,
            IsParent = false
        };
    }
}