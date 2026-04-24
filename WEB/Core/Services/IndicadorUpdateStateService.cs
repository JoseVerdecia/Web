namespace WEB.Core.Services;

public class IndicadorUpdateStateService
{
    public event Action? OnIndicadorUpdated;

    public void NotifyUpdate()
    {
        OnIndicadorUpdated?.Invoke();
    }
}