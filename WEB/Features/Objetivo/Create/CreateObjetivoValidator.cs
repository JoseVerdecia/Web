using FluentValidation;

namespace WEB.Features.Objetivo.Create;

public class CreateObjetivoValidator : AbstractValidator<CreateObjetivoCommand>
{
    public CreateObjetivoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del objetivo es obligatorio")
            .MaximumLength(150);
    }
}