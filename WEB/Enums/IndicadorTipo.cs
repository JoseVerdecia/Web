using System.ComponentModel.DataAnnotations;

namespace WEB.Enums;

public enum IndicadorTipo
{
    [Display(Name = "Escencial")]
    Escencial,
    
    [Display(Name = "Necesario")]
    Necesario
}