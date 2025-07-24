using FluentValidation;
using viaggia_server.DTOs.Users;

namespace viaggia_server.Validators
{
    public class CreateAttendantDTOValidator : AbstractValidator<CreateAttendantDTO>
    {
        public CreateAttendantDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.EmployerCompanyName)
                .NotEmpty().WithMessage("Employer company name is required.")
                .MaximumLength(100).WithMessage("Employer company name cannot exceed 100 characters.");

            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required.")
                .MaximumLength(50).WithMessage("Employee ID cannot exceed 50 characters.");

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
        }
    }
}