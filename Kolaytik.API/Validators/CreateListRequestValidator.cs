using FluentValidation;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.API.Validators;

public class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Liste adı zorunludur.")
            .MinimumLength(2).WithMessage("Liste adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Liste adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.")
            .When(x => x.Description is not null);
    }
}
