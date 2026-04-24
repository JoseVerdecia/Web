using System.ComponentModel.DataAnnotations;
using WEB.Core.Helpers;

namespace WEB.Enums;

public enum Evaluacion
{
    [Display(Name = "No Evaluado")]
    [BadgeColor("#9C9C9C")]
    NoEvaluado,
    
    [Display(Name = "Sobrecumplido")]
    [BadgeColor("#037036")]
    Sobrecumplido,
    
    [Display(Name = "Cumplido")]
    [BadgeColor("#05B353")]
    Cumplido,
    
    [Display(Name = "Parcialmente")]
    [BadgeColor("#f97316")]
    ParcialmenteCumplido,
    
    [Display(Name = "Incumplido")]
    [BadgeColor("#dc2626")]
    Incumplido
}