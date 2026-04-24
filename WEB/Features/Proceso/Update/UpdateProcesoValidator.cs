using FluentValidation;

namespace WEB.Features.Proceso.Update;

public class UpdateProcesoValidator : AbstractValidator<UpdateProcesoCommand>
{
    public UpdateProcesoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID del proceso debe ser mayor a 0");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del proceso es requerido")
            .MinimumLength(2)
            .WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");
    }
}