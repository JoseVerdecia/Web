using System.ComponentModel.DataAnnotations;
using WEB.Enums;
using WEB.Core.Interfaces;

namespace WEB.Models;

public class IndicadorModel:ISoftDeletable,ICreatedInterfaces
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Nombre { get; set; } = null!;
    
    // Propiedades de Meta a Cumplir
    [Required]
    public string MetaCumplir { get; set; } = null!;
    public decimal MetaCumplirDecimal { get; set; }
    public bool IsMetaCumplirPorcentaje { get; set; } = false;
    
    // Propiedades de Meta Real
    public string? MetaReal { get; set; }
    public decimal MetaRealDecimal { get; set; }
   
    public bool IsMetaRealPorcentaje { get; set; } = false;
    
    public decimal? ValorTotalAcumulado { get; set; }
    public decimal? ValorRealAcumulado { get; set; }
    // Enums
    [Required]
    public IndicadorOrigen Origen { get; set; }
    
    [Required]
    public IndicadorTipo Tipo { get; set; }
    
    public Evaluacion Evaluacion { get; set; } = Evaluacion.NoEvaluado;
    
    public string? Observacion { get; set; }

    // Relaciones
    public int ProcesoId { get; set; }
    public ProcesoModel Proceso { get; set; } = null!;
    public List<ObjetivoModel> Objetivos { get; set; } = new(); 
    public List<IndicadorDeAreaModel> IndicadoresDeArea { get; set; } = new();
    
    // Interfaces 
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}