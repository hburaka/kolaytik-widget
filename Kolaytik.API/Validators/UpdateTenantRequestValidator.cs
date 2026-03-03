using FluentValidation;
using Kolaytik.Core.DTOs.Tenant;
using Kolaytik.Core.Enums;

namespace Kolaytik.API.Validators;

public class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Firma adı zorunludur.")
            .MinimumLength(2).WithMessage("Firma adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Firma adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçersiz firma durumu.");
    }
}
