namespace WEB.Common;

public class ObjetivoSeleccionable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int NumeroObjetivo { get; set; }
    public bool IsSelected { get; set; }
}