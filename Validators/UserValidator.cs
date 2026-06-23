using FluentValidation;
using RentingBooking.Models;

namespace RentingBooking.Validators;

public class UserValidator : AbstractValidator<User>
{
    public  UserValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("El nombre de usuario no puede estar vacío.")
            .Length(3, 50).WithMessage("El nombre de usuario debe tener entre 3 y 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_\.]+$").WithMessage("El nombre de usuario solo permite letras, números, puntos o guiones bajos.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(150).WithMessage("El correo electrónico no puede superar los 150 caracteres.");

        RuleFor(u => u.PasswordHash)
            .NotEmpty().WithMessage("El hash de la contraseña es requerido para la persistencia.")
            .MaximumLength(255).WithMessage("El hash de la contraseña no puede superar los 255 caracteres.");

        RuleFor(u => u.Role)
            .IsInEnum().WithMessage("El rol asignado no es un rol válido en el sistema.");

        RuleFor(u => u.CreatedAt)
            .NotEmpty().WithMessage("La fecha de creación es obligatoria.")
            .LessThanOrEqualTo(p => DateTime.UtcNow).WithMessage("La fecha de creación no puede ser en el futuro.");
    }
    
}