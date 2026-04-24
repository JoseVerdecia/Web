using WEB.Core.Mediator;
using WEB.Data;
using WEB.Enums;
using WEB.Features.Indicador.Dto;

namespace WEB.Features.Indicador.Update;

public record UpdateIndicadorCommand(
    int Id,
    string Nombre,
    string MetaCumplir,
    string? MetaReal,
    IndicadorOrigen Origen,
    IndicadorTipo Tipo,
    string? Observacion,
    int ProcesoId,
    List<int>? ObjetivoIds,
    Dictionary<int, string>? MetaCumplirPorArea
) : IRequest<IndicadorDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador, AppRoles.JefeProceso };
}

