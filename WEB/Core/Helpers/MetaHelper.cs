using System.Text.RegularExpressions;

namespace WEB.Core.Helpers;

public static class MetaHelper
{
    private static readonly Regex NumeroValidoRegex = new Regex(@"^\d+(\.\d+)?$", RegexOptions.Compiled);
    /// <summary>
    /// Intenta parsear una cadena de texto a un valor decimal de meta.
    /// Soporta formatos: "90", "90.02", "90%", "90.02%".
    /// </summary>
    /// <param name="meta">Cadena de entrada.</param>
    /// <param name="value">Valor decimal resultante.</param>
    /// <param name="isPorcentaje">Indica si la meta es un porcentaje.</param>
    /// <returns>True si el parseo fue exitoso, False en caso contrario.</returns>
    public static bool TryParsearMeta(string? meta, out decimal value, out bool isPorcentaje)
    {
        value = 0;
        isPorcentaje = false;

        if (string.IsNullOrWhiteSpace(meta))
            return false;

        meta = meta.Trim();
        
        if (meta.EndsWith("%"))
        {
            isPorcentaje = true;
            meta = meta.Substring(0, meta.Length - 1).Trim();
        }
   
        if (!NumeroValidoRegex.IsMatch(meta))
            return false;
        
        return decimal.TryParse(meta, System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }
    
    public static string SincronizarMetaReal(string metaCumplir, string metaReal)
    {
        if (metaCumplir.Trim().EndsWith("%") && !metaReal.Trim().EndsWith("%"))
            return metaReal + "%";
        if (!metaCumplir.Trim().EndsWith("%") && metaReal.Trim().EndsWith("%"))
            return metaReal.TrimEnd('%');
        return metaReal;
    }
}