using System.ComponentModel.DataAnnotations;
using WEB.Data;
using WEB.Enums;
using WEB.Interfaces;

namespace WEB.Models;

public class AreaModel:ISoftDeletable,ICreatedInterfaces
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Nombre { get; set; } = null!;
    
    public string? JefeAreaId { get; set; } 
    
    public ApplicationUser? JefeArea { get; set; }
    public AreaTipo Tipo { get; set; } = AreaTipo.Facultad;
    public List<IndicadorDeAreaModel> IndicadoresDeArea { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}