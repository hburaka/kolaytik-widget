using FluentValidation;
using Kolaytik.Core.DTOs.User;
using Kolaytik.Core.Enums;

namespace Kolaytik.API.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-mail adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçersiz rol.");
    }
}
