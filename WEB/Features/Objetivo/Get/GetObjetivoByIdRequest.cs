using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Objetivo.Get;

public record GetObjetivoByIdRequest(int Id) : IRequest<ObjetivoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}