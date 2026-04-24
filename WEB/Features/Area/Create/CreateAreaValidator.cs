using FluentValidation;

namespace WEB.Features.Area.Create;

public class CreateAreaValidator : AbstractValidator<CreateAreaCommand>
{
    public CreateAreaValidator()
    {
        
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del área es obligatorio")
            .MaximumLength(50)
            .NotNull().WithMessage("El nombre del Area no puede ser null");
        RuleFor(x=> x.Tipo)
            .NotNull().WithMessage("El tipo es obligatorio")
            .IsInEnum().WithMessage("Formato invalido de Tipo de Area");
    }
}