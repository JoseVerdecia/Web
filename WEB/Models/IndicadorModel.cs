using System.ComponentModel.DataAnnotations;
using WEB.Enums;
using WEB.Interfaces;

namespace WEB.Models;

public class IndicadorModel:ISoftDeletable,ICreatedInterfaces
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Nombre { get; set; } = null!;
    
    [Required]
    public string MetaCumplir { get; set; } = null!;
    public decimal MetaCumplirDecimal { get; set; }
    
    [Required]
    public IndicadorOrigen Origen { get; set; }
    [Required]
    public IndicadorTipo Tipo { get; set; }
    public string? MetaReal { get; set; }
    public decimal MetaRealDecimal { get; set; }
    public bool IsMetaCumplirPorcentaje { get; set; } = false;
    public bool IsMetaRealPorcentaje { get; set; } = false;
    public string? Observacion { get; set; }
    public Evaluacion Evaluacion { get; set; } = Evaluacion.NoEvaluado;

    public int ProcesoId { get; set; }
    public ProcesoModel Proceso { get; set; } = null!;
    public List<ObjetivoModel> Objetivos { get; set; } = new(); 
    public List<IndicadorDeAreaModel> IndicadoresDeArea { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}