using FluentValidation;
using viaggia_server.DTOs.Users;

namespace viaggia_server.Validators
{
    public class CreateClientDTOValidator : AbstractValidator<CreateClientDTO>
    {
        public CreateClientDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(256).WithMessage("Password cannot exceed 256 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF is required.")
                .Matches(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$").WithMessage("CPF must be in the format 123.456.789-00.");
        }
    }
}