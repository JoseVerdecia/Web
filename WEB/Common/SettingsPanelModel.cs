using Microsoft.FluentUI.AspNetCore.Components;

namespace WEB.Common;

public class SettingsPanelModel
{
    public DesignThemeModes Mode { get; set; } = DesignThemeModes.System;
    public ThemeColorOption SelectedColorOption { get; set; } = default!;
}