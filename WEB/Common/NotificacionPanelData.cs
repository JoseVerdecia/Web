namespace WEB.Common;

public record NotificacionPanelData
{
    public string UsuarioId { get; init; } = null!;
    public int CountNoLeidas { get; init; }
}