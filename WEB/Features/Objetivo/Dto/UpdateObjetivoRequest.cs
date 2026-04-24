using System.ComponentModel.DataAnnotations;

namespace WEB.Features.Objetivo.Dto;

public class UpdateObjetivoRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; }
    
    [Required(ErrorMessage = "El numero de objetivo es obligatorio")]
    public int NumeroObjetivo { get; set; }
    
}