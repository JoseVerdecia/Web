using System.Globalization;
using WEB.Features.Indicador.Dto;

namespace WEB.Core.Extensions;

public static class DecimalExtensions
{ 
        /// <summary>
        /// Formatea un número decimal con separadores de miles.
        /// Ejemplo: 1522054 → "1,522,054"
        /// </summary>
        public static string FormatearMiles(this decimal value) => value.ToString("N0", CultureInfo.InvariantCulture);
        
        
        /// <summary>
        /// Calcula el porcentaje entre un valor total y un valor real: (real / total) * 100.
        /// 
        /// Se usa en la Tabla 2 (Valores Cuantitativos) del Excel SIGES:
        ///   - Fila 1: Valor Total (datos del año anterior o base)
        ///   - Fila 2: Valor Real (datos del año en curso)
        ///   - Porcentaje = ValorReal / ValorTotal * 100
        ///   
        /// Ejemplo: Total=1522054, Real=1371954 → 90.14%
        /// </summary>
        public static (string Text, string Color) CalcularPorcentaje(this decimal? total, decimal? real)
        {
                if (!total.HasValue || total.Value == 0)
                        return ("—", "#9C9C9C");

                if (!real.HasValue)
                        return ("—", "#9C9C9C");

                var porcentaje = (real.Value / total.Value) * 100;

                var color = porcentaje switch
                {
                        >= 100 => "#037036",
                        >= 80  => "#05B353",
                        >= 50  => "#f97316",
                        _      => "#dc2626"
                };

                return ($"{porcentaje:F2}%", color);
        }
        
        /// <summary>
        /// Calcula el % de cumplimiento de un indicador: (MetaRealDecimal / MetaCumplirDecimal) * 100.
        /// 
        /// Se usa en la Tabla 1 del Excel SIGES, columna "% CUMPLIMIENTO".
        /// Compara lo que se alcanzó (MetaReal) vs lo que se había planeado (MetaCumplir).
        /// 
        /// Umbrales de color:
        ///   > 100%  → Verde oscuro (Sobrecumplido)
        ///   = 100%  → Verde (Cumplido)
        ///   ≥ 80%   → Naranja (Parcialmente cumplido)
        ///   &lt; 80%   → Rojo (Incumplido)
        /// </summary>
        public static (string Text, string Color) CalcularPorcentajeCumplimiento(this IndicadorDto ind)
        {
                if (ind.MetaCumplirDecimal == 0 || ind.MetaRealDecimal == 0)
                        return ("0.00%", "#dc2626");

                var porcentaje = (ind.MetaRealDecimal / ind.MetaCumplirDecimal) * 100;

                var color = porcentaje switch
                {
                        > 100 => "#037036",
                        100   => "#05B353",
                        >= 80 => "#f97316",
                        _     => "#dc2626"
                };

                return ($"{porcentaje:F2}%", color);
        }
}