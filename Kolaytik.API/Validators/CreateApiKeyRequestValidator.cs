using FluentValidation;
using Kolaytik.Core.DTOs.ApiKey;

namespace Kolaytik.API.Validators;

public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("API anahtar adı zorunludur.")
            .MinimumLength(2).WithMessage("API anahtar adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("API anahtar adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.RateLimitPerMinute)
            .InclusiveBetween(1, 1000).WithMessage("Dakika başı istek limiti 1-1000 arasında olmalıdır.");

        RuleFor(x => x.RateLimitPerDay)
            .InclusiveBetween(100, 100000).WithMessage("Günlük istek limiti 100-100000 arasında olmalıdır.");
    }
}
