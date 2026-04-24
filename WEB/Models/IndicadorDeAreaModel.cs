using WEB.Enums;
using WEB.Interfaces;

namespace WEB.Models;

public class IndicadorDeAreaModel:ISoftDeletable
{
    public int Id { get; set; }
    public int IndicadorId { get; set; }
    public IndicadorModel Indicador { get; set; } = null!;
    public int AreaId { get; set; }
    public AreaModel Area { get; set; } = null!;
    public string? MetaReal { get; set; }
    public decimal MetaRealDecimal { get; set; }
    public List<NotificacionModel> Notificaciones { get; set; } = new();
    public bool IsMetaRealPorcentaje { get; set; } = false;
    
    public string? MetaCumplir { get; set; }
    public decimal MetaCumplirDecimal { get; set; }
    public bool IsMetaCumplirPorcentaje { get; set; } = false;
    public Evaluacion Evaluacion { get; set; } = Evaluacion.NoEvaluado;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}