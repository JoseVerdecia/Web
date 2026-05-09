using WEB.Features.Notificacion.Dto;

namespace WEB.Core.Services;

public class NotificacionStateService
{
    
    public int CountNoLeidas { get; private set; }


    public List<NotificacionDto> Notificaciones { get; private set; } = new();


    public event Action? OnChange;
    public event Action? OnCountChanged; 
    
    public void UpdateCount(int count)
    {
        CountNoLeidas = count;
        OnCountChanged?.Invoke(); 
        OnChange?.Invoke();       
    }
    
    public void SetNotificaciones(List<NotificacionDto> notificaciones)
    {
        Notificaciones = notificaciones;
        Notify();
    }
    
    public void AddNotificacion(NotificacionDto notif)
    {
        if (Notificaciones.Any(n => n.Id == notif.Id))
            return;

        Notificaciones.Insert(0, notif);
        Notify();
    }

    public void UpdateNotificacion(NotificacionDto notif)
    {
        var index = Notificaciones.FindIndex(n => n.Id == notif.Id);
        if (index >= 0)
        {
            Notificaciones[index] = notif;
            Notify();
        }
    }

    public void RemoveNotificacion(int id)
    {
        var removed = Notificaciones.RemoveAll(n => n.Id == id) > 0;
        if (removed)
            Notify();
    }


    public void Clear()
    {
        Notificaciones.Clear();
        Notify();
    }

    private void Notify()
    {
        OnChange?.Invoke();
    }
}