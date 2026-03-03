using FluentValidation;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.API.Validators;

public class UpdateListItemRequestValidator : AbstractValidator<UpdateListItemRequest>
{
    public UpdateListItemRequestValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Etiket zorunludur.")
            .MaximumLength(500).WithMessage("Etiket en fazla 500 karakter olabilir.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Değer zorunludur.")
            .MaximumLength(500).WithMessage("Değer en fazla 500 karakter olabilir.");

        RuleFor(x => x.OrderIndex)
            .GreaterThan(0).WithMessage("Sıra numarası 0'dan büyük olmalıdır.");
    }
}
