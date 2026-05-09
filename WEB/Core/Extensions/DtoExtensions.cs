using WEB.Features.Indicador.Dto;

namespace WEB.Core.Extensions;

public static class DtoExtensions
{
    public static (string Text, string Color) GetTextAndColor(this IndicadorDto dto)
    {
        if (dto.MetaCumplirDecimal == 0 || dto.MetaRealDecimal == 0)
            return ("0.00%", "#dc2626");

        var porcentaje = (dto.MetaRealDecimal / dto.MetaCumplirDecimal) * 100;
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