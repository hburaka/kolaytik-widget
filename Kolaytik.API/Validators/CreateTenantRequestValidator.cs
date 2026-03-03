using FluentValidation;
using Kolaytik.Core.DTOs.Tenant;

namespace Kolaytik.API.Validators;

public class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Firma adı zorunludur.")
            .MinimumLength(2).WithMessage("Firma adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Firma adı en fazla 200 karakter olabilir.");
    }
}
