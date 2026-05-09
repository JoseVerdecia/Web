namespace WEB.Core.Extensions;

public static class DoubleExtensions
{
    public static string FormatDoublePorcentaje(this double valor)
    {
        if (valor == 0) return "—";
        return valor.ToString("F2") + "%";
    }
}