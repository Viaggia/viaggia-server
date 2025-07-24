using FluentValidation;
using viaggia_server.DTOs.Users;

public class CreateAdminDTOValidator : AbstractValidator<CreateAdminDTO>
{
    public CreateAdminDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.PhoneNumber).NotEmpty();
    }
}
