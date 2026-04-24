using FluentValidation;

namespace WEB.Features.Objetivo.Update;

public class UpdateObjetivoValidator : AbstractValidator<UpdateObjetivoCommand>
{
    public UpdateObjetivoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID del objetivo debe ser mayor a 0");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del objetivo es requerido")
            .MinimumLength(2)
            .WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");
        
        RuleFor(x => x.NumeroObjetivo)
            .GreaterThan(0)
            .WithMessage("El número de objetivo debe ser mayor a cero");
    }
}

