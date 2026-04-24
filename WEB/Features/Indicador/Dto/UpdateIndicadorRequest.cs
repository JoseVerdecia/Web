using System.ComponentModel.DataAnnotations;
using WEB.Enums;

namespace WEB.Features.Indicador.Dto;

public class UpdateIndicadorRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Nombre {get; set; }
    
    [Required]
    public string MetaCumplir { get; set; }
    public string? MetaReal { get; set; }
    
    [Required]
    public IndicadorOrigen Origen { get; set; }
    
    [Required]
    public IndicadorTipo Tipo { get; set; }
    
    public string? Observacion { get; set; }
    
    [Required]
    public int ProcesoId { get; set; }
    
    [Required]
    public List<int> ObjetivoIds { get; set; }
    
    public Dictionary<int, string>? MetaCumplirPorArea { get; set; } 
}