using System.Globalization;
using Microsoft.AspNetCore.Components;
using WEB.Common;

namespace WEB.Components.Shared;

public partial class ResumenObjetivoTable : ComponentBase
{
    [Parameter] public List<ObjetivoResumenData> Datos { get; set; } = new();

    private string FormatearPorcentaje(double valor)
    {
        if (Math.Abs(valor) < 0.001) return "0,00%";
        return valor.ToString("F2", CultureInfo.InvariantCulture) + "%";
    }
}