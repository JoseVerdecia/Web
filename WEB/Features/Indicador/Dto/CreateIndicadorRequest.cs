using System.ComponentModel.DataAnnotations;

namespace WEB.Features.Indicador.Dto;

public class CreateIndicadorRequest
{
    [Required]
    public string Nombre {get; set; }
    
    [Required]
    public string MetaCumplir { get; set; }
    public string? MetaReal { get; set; }
    
    [Required]
    public string Origen { get; set; }
    
    [Required]
    public string Tipo { get; set; }
    public string? Observacion { get; set; }
    
    [Required]
    public int ProcesoId { get; set; }
    
    [Required]
    public List<int> ObjetivoIds { get; set; }
    
    public Dictionary<int, string>? MetaCumplirPorArea { get; set; } 
}