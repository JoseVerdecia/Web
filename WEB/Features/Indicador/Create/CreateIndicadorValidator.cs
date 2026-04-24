using FluentValidation;

namespace WEB.Features.Indicador.Create;

public class CreateIndicadorValidator : AbstractValidator<CreateIndicadorCommand>
{
    public CreateIndicadorValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.MetaCumplir).NotEmpty();
        RuleFor(x => x.ProcesoId).GreaterThan(0);
        RuleFor(x => x.ObjetivoIds).NotEmpty().WithMessage("Debe seleccionar al menos un objetivo");
        
    }
}