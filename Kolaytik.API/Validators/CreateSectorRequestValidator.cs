using FluentValidation;
using Kolaytik.Core.DTOs.Sector;

namespace Kolaytik.API.Validators;

public class CreateSectorRequestValidator : AbstractValidator<CreateSectorRequest>
{
    public CreateSectorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sektör adı zorunludur.")
            .MinimumLength(2).WithMessage("Sektör adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Sektör adı en fazla 100 karakter olabilir.");
    }
}
