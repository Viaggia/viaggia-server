using FluentValidation;
using viaggia_server.DTOs.Users;

namespace viaggia_server.Validators
{
    public class CreateServiceProviderDTOValidator : AbstractValidator<CreateServiceProviderDTO>
    {
        public CreateServiceProviderDTOValidator()
        {
            RuleFor(x => x.ResponsibleName)
                .NotEmpty().WithMessage("Responsible name is required.")
                .MaximumLength(100).WithMessage("Responsible name cannot exceed 100 characters.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters.");

            RuleFor(x => x.Cnpj)
                .NotEmpty().WithMessage("CNPJ is required.")
                .Matches(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$").WithMessage("CNPJ must be in the format 12.345.678/0001-99.");

            RuleFor(x => x.CompanyLegalName)
                .NotEmpty().WithMessage("Company legal name is required.")
                .MaximumLength(100).WithMessage("Company legal name cannot exceed 100 characters.");

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