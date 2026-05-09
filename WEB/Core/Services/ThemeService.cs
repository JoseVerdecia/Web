using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WEB.Core.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "theme";

    public event Action? OnChange;

    public DesignThemeModes Mode { get; set; } = DesignThemeModes.System;
    public OfficeColor? OfficeColor { get; set; } =  Microsoft.FluentUI.AspNetCore.Components.OfficeColor.Default;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task LoadInitialThemeAsync()
    {
        try
        {
            var saved = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(saved))
            {
                var themeData = System.Text.Json.JsonSerializer.Deserialize<ThemeData>(saved);
                Mode = themeData?.Mode ?? DesignThemeModes.System;
                OfficeColor = themeData?.PrimaryColor ?? Microsoft.FluentUI.AspNetCore.Components.OfficeColor.Default;
            }
        }
        catch
        {
            // Si falla, se mantienen los valores por defecto
        }
        NotifyStateChanged();
    }

    public void SetMode(DesignThemeModes mode)
    {
        Mode = mode;
        SaveAndNotify();
    }

    public void SetOfficeColor(OfficeColor? color)
    {
        OfficeColor = color;
        SaveAndNotify();
    }

    private async void SaveAndNotify()
    {
        var themeData = new { mode = Mode.ToString().ToLower(), primaryColor = OfficeColor?.ToString() };
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, System.Text.Json.JsonSerializer.Serialize(themeData));
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private class ThemeData
    {
        public DesignThemeModes Mode { get; set; }
        public OfficeColor? PrimaryColor { get; set; }
    }
}