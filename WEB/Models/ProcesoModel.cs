    using System.ComponentModel.DataAnnotations;
    using WEB.Data;
    using WEB.Enums;
    using WEB.Interfaces;

    namespace WEB.Models;

    public class ProcesoModel:ISoftDeletable,ICreatedInterfaces
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Nombre { get; set; } = null!;
        
        public Evaluacion Evaluacion { get; set; } = Evaluacion.NoEvaluado;
        public List<IndicadorModel> Indicadores { get; set; } = new();
        
        public string? JefeProcesoId { get; set; } 
        
        public ApplicationUser? JefeProceso { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }