using FluentValidation;
using viaggia_server.DTOs.User;

namespace viaggia_server.Validators
{
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{10,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Cpf)
                .Matches(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$").When(x => !string.IsNullOrEmpty(x.Cpf))
                .WithMessage("CPF must be in the format 123.456.789-00.");

            RuleFor(x => x.CompanyName)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.CompanyName))
                .WithMessage("Company name cannot exceed 100 characters.");

            RuleFor(x => x.CompanyLegalName)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.CompanyLegalName))
                .WithMessage("Company legal name cannot exceed 100 characters.");

            RuleFor(x => x.EmployerCompanyName)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.EmployerCompanyName))
                .WithMessage("Employer company name cannot exceed 100 characters.");

            RuleFor(x => x.EmployeeId)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.EmployeeId))
                .WithMessage("Employee ID cannot exceed 50 characters.");

            RuleFor(x => x.Avatar)
                .Must(file => file == null || IsValidImage(file))
                .WithMessage("Avatar must be a valid image file (jpg, jpeg, png) and not exceed 5MB.");
        }

        private bool IsValidImage(IFormFile? file)
        {
            if (file == null)
                return true; // Image is optional

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var maxSizeInBytes = 5 * 1024 * 1024; // 5MB

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension) && file.Length <= maxSizeInBytes;
        }
    }
}