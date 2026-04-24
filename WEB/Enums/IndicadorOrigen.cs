using System.ComponentModel.DataAnnotations;

namespace WEB.Enums;

public enum IndicadorOrigen
{
    [Display(Name = "Interno")]
    Interno,
    
    [Display(Name = "MES")]
    MES
}