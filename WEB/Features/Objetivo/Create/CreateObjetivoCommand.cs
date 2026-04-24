using WEB.Core.Mediator;
using WEB.Data;
using WEB.Features.Objetivo.Dto;

namespace WEB.Features.Objetivo.Create;

public record CreateObjetivoCommand(string Nombre,int NumeroObjetivo) : IRequest<ObjetivoDto>,IRequireAuthorization
{
    public string[] Roles => new[] { AppRoles.Administrador };
}