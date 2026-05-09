using System.ComponentModel.DataAnnotations;
using WEB.Core.Helpers;

namespace WEB.Enums;

public enum Evaluacion
{
    [Display(Name = "No Evaluado")]
    [BadgeColor("#9C9C9C")]
    NoEvaluado,
    
    [Display(Name = "Sobrecumplido")]
    [BadgeColor("#1B74B6")]
    Sobrecumplido,
    
    [Display(Name = "Cumplido")]
    [BadgeColor("#34B66B")]
    Cumplido,
    
    [Display(Name = "Parcialmente")]
    [BadgeColor("#F7BC20")]
    ParcialmenteCumplido,
    
    [Display(Name = "Incumplido")]
    [BadgeColor("#ED7425")]
    Incumplido
}