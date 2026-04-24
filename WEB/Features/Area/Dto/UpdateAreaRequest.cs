using System.ComponentModel.DataAnnotations;
using WEB.Enums;

namespace WEB.Features.Area.Dto;

public class UpdateAreaRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El tipo de area es requerido")]
    public AreaTipo Tipo { get; set; }
}