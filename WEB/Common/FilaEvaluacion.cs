namespace WEB.Common;

public class FilaEvaluacion
{
    public string Nombre { get; set; } = "";
    public int Total { get; set; }
    public int Sobrecumplidos { get; set; }
    public int Cumplidos { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public int Incumplidos { get; set; }
    public double PorcentajeS { get; set; }
    public double PorcentajeC { get; set; }
    public double PorcentajePC { get; set; }
    public double PorcentajeI { get; set; }
}