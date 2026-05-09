using System.ComponentModel.DataAnnotations;
using System.Reflection;
using WEB.Core.Helpers;
using WEB.Enums;

namespace WEB.Core.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<DisplayAttribute>();

        return attribute?.GetName() ?? value.ToString();
    }
    
    public static string GetBadgeColor(this Evaluacion evaluacion)
    {
        var field = evaluacion.GetType().GetField(evaluacion.ToString());
        var attribute = field?.GetCustomAttribute<BadgeColorAttribute>();
        return attribute?.Color ?? string.Empty;
    }
    
}