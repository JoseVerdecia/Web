using System.ComponentModel.DataAnnotations;
using WEB.Enums;
using WEB.Interfaces;

namespace WEB.Models;

public class ObjetivoModel:ISoftDeletable,ICreatedInterfaces
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Nombre { get; set; } = null!;
    
    [Required]
    public int NumeroObjetivo { get; set; }
    
    public Evaluacion Evaluacion { get; set; } = Evaluacion.NoEvaluado;
    public List<IndicadorModel> Indicadores { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}