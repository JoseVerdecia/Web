namespace WEB.Features.Dashboard.Dto;

public class EvaluacionConteoDto
{
    public int Sobrecumplidos { get; set; }
    public int Cumplidos { get; set; }
    public int ParcialmenteCumplidos { get; set; }
    public int Incumplidos { get; set; }
    public int NoEvaluados { get; set; }
    public int Total => Sobrecumplidos + Cumplidos + ParcialmenteCumplidos + Incumplidos + NoEvaluados;
}