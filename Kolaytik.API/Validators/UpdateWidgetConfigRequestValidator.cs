using FluentValidation;
using Kolaytik.Core.DTOs.Widget;

namespace Kolaytik.API.Validators;

public class UpdateWidgetConfigRequestValidator : AbstractValidator<UpdateWidgetConfigRequest>
{
    private static readonly string[] ValidWidths = ["25%", "50%", "75%", "100%"];

    public UpdateWidgetConfigRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Config adı zorunludur.")
            .MinimumLength(2).WithMessage("Config adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Config adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Width)
            .NotEmpty().WithMessage("Genişlik zorunludur.")
            .Must(w => ValidWidths.Contains(w)).WithMessage("Geçerli genişlik değerleri: 25%, 50%, 75%, 100%.");

        RuleFor(x => x.Levels)
            .NotNull().WithMessage("En az bir level tanımlanmalıdır.")
            .Must(l => l != null && l.Count >= 1).WithMessage("En az bir level tanımlanmalıdır.");

        RuleForEach(x => x.Levels).SetValidator(new WidgetConfigLevelDtoValidator());
    }
}
