namespace WEB.Common;

public record NotificacionSignalRDto
{
    public int Id { get; init; }
    public string Cabecera { get; init; } = null!;
    public string Cuerpo { get; init; } = null!;
    public string Tipo { get; init; } = null!;
    public string Estado { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public string? RemitenteNombre { get; init; }
    public int CountNoLeidas { get; init; }
}