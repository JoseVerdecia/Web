using FluentValidation;

namespace WEB.Features.Indicador.Update;

public class UpdateIndicadorValidator : AbstractValidator<UpdateIndicadorCommand>
{
    public UpdateIndicadorValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID del indicador debe ser mayor a 0");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del indicador es requerido")
            .MinimumLength(2)
            .WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.MetaCumplir)
            .NotEmpty()
            .WithMessage("La meta a cumplir es requerida");

        RuleFor(x => x.Origen)
            .IsInEnum()
            .WithMessage("El origen del indicador es inválido");

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("El tipo de indicador es inválido");

        RuleFor(x => x.ProcesoId)
            .GreaterThan(0)
            .WithMessage("El ID del proceso debe ser mayor a 0");

        RuleFor(x => x.ObjetivoIds)
            .NotNull()
            .Must(x => x.Count > 0)
            .WithMessage("Debe proporcionar al menos un objetivo");
    }
}

