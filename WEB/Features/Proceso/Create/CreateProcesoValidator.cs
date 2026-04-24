using FluentValidation;

namespace WEB.Features.Proceso.Create;

public class CreateProcesoValidator : AbstractValidator<CreateProcesoCommand>
{
    public CreateProcesoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del proceso es obligatorio")
            .MaximumLength(100);
    }
}