using Microsoft.FluentUI.AspNetCore.Components;

namespace WEB.Common;

public class ThemeColorOption
{
    public string DisplayName { get; set; } = string.Empty;
    public OfficeColor? OfficeColor { get; set; }
    public string? CustomColorHex { get; set; }

    public bool IsCustom => !string.IsNullOrEmpty(CustomColorHex);
    public string Value => OfficeColor?.ToString() ?? CustomColorHex ?? "default";

    public static ThemeColorOption FromOfficeColor(OfficeColor color, string displayName) =>
        new() { OfficeColor = color, DisplayName = displayName };

    public static ThemeColorOption FromCustomColor(string hex, string displayName)
    {
        return new ThemeColorOption { CustomColorHex = hex, DisplayName = displayName };
    }
}