using FluentValidation;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.API.Validators;

public class ReorderItemsRequestValidator : AbstractValidator<ReorderItemsRequest>
{
    public ReorderItemsRequestValidator()
    {
        RuleFor(x => x.ItemIds)
            .NotEmpty().WithMessage("Sıralanacak eleman listesi boş olamaz.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Eleman listesinde tekrar eden ID'ler olamaz.");
    }
}
