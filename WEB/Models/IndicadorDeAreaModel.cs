using WEB.Enums;
using WEB.Core.Interfaces;

namespace WEB.Models;

public class IndicadorDeAreaModel:ISoftDeletable
{
    public int Id { get; set; }
    
    // Relacion con su Indicador
    public int IndicadorId { get; set; }
    
    public IndicadorTipo IndicadorPadreTipo { get; set; }
    public IndicadorModel Indicador { get; set; } = null!;
    
    // Relacion con su Area
    public int AreaId { get; set; }
    public AreaModel Area { get; set; } = null!;
    
    // Meta Real del Indicador del Area
    public string? MetaReal { get; set; }
    public decimal MetaRealDecimal { get; set; }
    public bool IsMetaRealPorcentaje { get; set; } = false;
    
    // Meta Cumplir del Indicador del Area
    public string? MetaCumplir { get; set; }
    public decimal MetaCumplirDecimal { get; set; }
    public bool IsMetaCumplirPorcentaje { get; set; } = false;
    
    // Valores Cuantitativos del Indicador del Area
    
    public decimal? ValorTotal { get; set; }
    public decimal? ValorReal { get; set; }
    
    // Valor Cualitativo del Indicador del Area
    public string? ValorCualitativo {get;set;}
    
    // Relacion con Notificaciones
    public List<NotificacionModel> Notificaciones { get; set; } = new();
    
    public Evaluacion Evaluacion { get; set; } = Evaluacion.NoEvaluado;
    
    // Interfaces
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}