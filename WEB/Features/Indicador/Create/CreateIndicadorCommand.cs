using WEB.Core.Mediator;
using WEB.Data;
using WEB.Enums;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.Create;

public record CreateIndicadorCommand(
    string Nombre,
    string MetaCumplir,
    IndicadorOrigen Origen,
    IndicadorTipo Tipo,
    string? Observacion,
    int ProcesoId,
    List<int> ObjetivoIds,
    string? ValorTotal,       
    string? ValorReal,
    Dictionary<int, string>? MetaCumplirPorArea // key: AreaId, value: MetaReal
) : IRequest<IndicadorDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador, AppRoles.JefeProceso };
}