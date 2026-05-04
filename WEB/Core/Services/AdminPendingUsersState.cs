namespace WEB.Core.Services;

public class AdminPendingUsersState
{
    public int PendingCount { get; private set; }
    public event Action? OnChange;

    public void SetCount(int count)
    {
        PendingCount = count;
        NotifyStateChanged();
    }

    public void Increment()
    {
        PendingCount++;
        NotifyStateChanged();
    }

    public void Reset()
    {
        PendingCount = 0;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}