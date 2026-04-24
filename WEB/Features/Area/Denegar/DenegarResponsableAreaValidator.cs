using FluentValidation;

namespace WEB.Features.Area.Denegar;

public class DenegarResponsableAreaValidator:AbstractValidator<DenegarResponsableAreaRequest>
{
    public DenegarResponsableAreaValidator()
    {
        RuleFor(x => x.AreaId).NotNull().WithMessage("El area no puede ser null");
        RuleFor(x=>x.JefeAreaId).NotNull().WithMessage("El responsable del area no puede ser null");
        RuleFor(x=>x.JefeAreaId).NotEmpty().WithMessage("El identificador del responsable no puede ser 'empty'");
    }
}