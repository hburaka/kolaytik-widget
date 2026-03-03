using FluentValidation;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.API.Validators;

public class SetRelationsRequestValidator : AbstractValidator<SetRelationsRequest>
{
    public SetRelationsRequestValidator()
    {
        RuleFor(x => x.ChildItemIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Child eleman listesinde tekrar eden ID'ler olamaz.")
            .When(x => x.ChildItemIds.Count > 0);
    }
}
