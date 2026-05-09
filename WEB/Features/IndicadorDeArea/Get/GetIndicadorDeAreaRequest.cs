using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.IndicadorDeArea.Dto;

namespace WEB.Features.IndicadorDeArea.Get;

public record GetIndicadorDeAreaRequest(int id):IRequest<IndicadorDeAreaDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.JefeArea,AppRoles.JefeProceso };
}