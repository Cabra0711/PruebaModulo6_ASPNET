using FluentValidation;
using RentingBooking.Models;

namespace RentingBooking.Validators;

public class PropertyValidator : AbstractValidator<Property>
{
    public PropertyValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(120).WithMessage("El título no puede superar los 120 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("La ubicación es obligatoria.")
            .MaximumLength(200).WithMessage("La ubicación no puede superar los 200 caracteres.");

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("El precio por noche debe ser mayor a 0.")
            .LessThanOrEqualTo(1000000).WithMessage("El precio por noche excede el límite permitido.");
        
        RuleFor(x => x.CreatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("La fecha de creación no puede ser futura.");

        RuleFor(x => x.Images)
            .NotNull().WithMessage("La colección de imágenes no puede ser nula.")
            .Must(imgs => imgs.Count <= 10)
            .WithMessage("No se permiten más de 10 imágenes por propiedad.");
        
    }
}