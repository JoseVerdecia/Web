using System.ComponentModel.DataAnnotations;

namespace WEB.Enums;

public enum AreaTipo
{
    [Display(Name = "Facultad")]
    Facultad,
    
    [Display(Name = "CUM/FUM")]
    CUMFUM,
    
    [Display(Name = "Direccion General")]
    DireccionGeneral,
    
    [Display(Name = "Departamento")]
    Departamento,
}