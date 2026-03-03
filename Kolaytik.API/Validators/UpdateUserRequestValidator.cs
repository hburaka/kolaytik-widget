using FluentValidation;
using Kolaytik.Core.DTOs.User;

namespace Kolaytik.API.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçersiz rol.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçersiz durum.");
    }
}
