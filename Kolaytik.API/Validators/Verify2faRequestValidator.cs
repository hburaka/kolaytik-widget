using FluentValidation;
using Kolaytik.Core.DTOs.Auth;

namespace Kolaytik.API.Validators;

public class Verify2faRequestValidator : AbstractValidator<Verify2faRequest>
{
    public Verify2faRequestValidator()
    {
        RuleFor(x => x.PreAuthToken)
            .NotEmpty().WithMessage("Oturum bilgisi eksik.");

        RuleFor(x => x.TotpCode)
            .NotEmpty().WithMessage("Doğrulama kodu zorunludur.")
            .Matches(@"^\d{6}$").WithMessage("Doğrulama kodu 6 haneli rakamdan oluşmalıdır.");
    }
}
