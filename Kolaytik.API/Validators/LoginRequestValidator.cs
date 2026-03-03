using FluentValidation;
using Kolaytik.Core.DTOs.Auth;

namespace Kolaytik.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .MaximumLength(128);

        RuleFor(x => x.TotpCode)
            .Matches(@"^\d{6}$").WithMessage("Doğrulama kodu 6 haneli rakamdan oluşmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.TotpCode));
    }
}
