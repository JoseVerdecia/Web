using FluentValidation;

namespace WEB.Features.Proceso.Assign;

public class AsignarResponsableProcesoValidator:AbstractValidator<AsignarResponsableProcesoRequest>
{
    public AsignarResponsableProcesoValidator()
    {
        RuleFor(x => x.ProcesoId).NotNull().WithMessage("El proceso no puede ser null");
        RuleFor(x=>x.UsuarioId).NotNull().WithMessage("El responsable del proceso no puede ser null");
        RuleFor(x=>x.UsuarioId).NotEmpty().WithMessage("El identificador del responsable no puede ser 'empty'");
    }
}