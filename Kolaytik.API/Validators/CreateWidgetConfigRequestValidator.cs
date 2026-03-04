using FluentValidation;
using Kolaytik.Core.DTOs.Widget;

namespace Kolaytik.API.Validators;

public class CreateWidgetConfigRequestValidator : AbstractValidator<CreateWidgetConfigRequest>
{
    private static readonly string[] ValidWidths = ["25%", "50%", "75%", "100%"];
    private static readonly string[] ValidElementTypes = ["Dropdown", "RadioButton", "CheckboxGroup", "MultiSelectDropdown"];

    public CreateWidgetConfigRequestValidator()
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

public class WidgetConfigLevelDtoValidator : AbstractValidator<WidgetConfigLevelDto>
{
    private static readonly string[] ValidElementTypes = ["Dropdown", "RadioButton", "CheckboxGroup", "MultiSelectDropdown"];

    public WidgetConfigLevelDtoValidator()
    {
        RuleFor(x => x.ListId)
            .NotEmpty().WithMessage("Liste seçimi zorunludur.");

        RuleFor(x => x.ElementType)
            .NotEmpty().WithMessage("Element tipi zorunludur.")
            .Must(t => ValidElementTypes.Contains(t)).WithMessage("Geçerli element tipleri: Dropdown, RadioButton, CheckboxGroup, MultiSelectDropdown.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Etiket zorunludur.")
            .MaximumLength(200).WithMessage("Etiket en fazla 200 karakter olabilir.");

        RuleFor(x => x.Placeholder)
            .MaximumLength(200).WithMessage("Placeholder en fazla 200 karakter olabilir.")
            .When(x => x.Placeholder != null);

        RuleFor(x => x.MaxSelections)
            .GreaterThan(0).WithMessage("Maksimum seçim sayısı 0'dan büyük olmalıdır.")
            .When(x => x.MaxSelections.HasValue);
    }
}
