namespace WEB.Common;

public class FilaResumen
{
    public string Nombre { get; set; } = "";
    public int Total { get; set; }
    public int Sobrecumplidos { get; set; }
    public double PorcentajeS { get; set; }
    public int Cumplidos { get; set; }
    public double PorcentajeC { get; set; }
    public int SCplusC { get; set; }
    public double PorcentajeSCplusC { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public double PorcentajePC { get; set; }
    public int SCplusCplusPC { get; set; }
    public double PorcentajeSCplusCplusPC { get; set; }
    public int Incumplidos { get; set; }
    public double PorcentajeI { get; set; }
    public int NoEvaluados { get; set; }
}