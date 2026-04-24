using System.ComponentModel.DataAnnotations;
using WEB.Core.Helpers;

namespace WEB.Enums;

public enum IndicadorTipo
{
    [Display(Name = "Escencial")]
    [BoldName]
    Escencial,
    
    [Display(Name = "Necesario")]
    Necesario
}