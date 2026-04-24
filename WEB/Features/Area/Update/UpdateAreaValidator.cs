using FluentValidation;

namespace WEB.Features.Area.Update;

public class UpdateAreaValidator : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID del área debe ser mayor a 0");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre del área es requerido")
            .MinimumLength(2)
            .WithMessage("El nombre debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("El tipo de área es inválido");
    }
}