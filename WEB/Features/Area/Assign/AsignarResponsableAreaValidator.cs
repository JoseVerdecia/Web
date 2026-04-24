using FluentValidation;

namespace WEB.Features.Area.Assign;

public class AsignarResponsableAreaValidator:AbstractValidator<AsignarResponsableAreaRequest>
{
    public AsignarResponsableAreaValidator()
    {
        RuleFor(x => x.AreaId).NotNull().WithMessage("El area no puede ser null");
        RuleFor(x=>x.UsuarioId).NotNull().WithMessage("El responsable del area no puede ser null");
        RuleFor(x=>x.UsuarioId).NotEmpty().WithMessage("El identificador del responsable no puede ser 'empty'");
    }
}