using FluentValidation;

namespace WEB.Features.Proceso.Denegar;

public class DenegarResponsableValidator:AbstractValidator<DenegarResponsableRequest>
{
    public DenegarResponsableValidator()
    {
        RuleFor(x => x.ProcesoId).NotNull().WithMessage("El proceso no puede ser null");
        RuleFor(x=>x.JefeProcesoId).NotNull().WithMessage("El responsable del proceso no puede ser null");
        RuleFor(x=>x.JefeProcesoId).NotEmpty().WithMessage("El identificador del responsable no puede ser 'empty'");
    }
}