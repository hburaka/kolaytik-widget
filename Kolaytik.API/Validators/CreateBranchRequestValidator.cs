using FluentValidation;
using Kolaytik.Core.DTOs.Branch;

namespace Kolaytik.API.Validators;

public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şube adı zorunludur.")
            .MinimumLength(2).WithMessage("Şube adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Şube adı en fazla 200 karakter olabilir.");
    }
}
