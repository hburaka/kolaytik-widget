using FluentValidation.TestHelper;
using Kolaytik.API.Validators;
using Kolaytik.Core.DTOs.User;
using Kolaytik.Core.Enums;

namespace Kolaytik.Tests.Unit.Validators;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    private static CreateUserRequest Valid() => new()
    {
        Email    = "user@kolaytik.com",
        Password = "StrongPass1",
        Role     = UserRole.TenantAdmin
    };

    [Fact]
    public void Valid_Request_Passes()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign")]
    public void Invalid_Email_Fails(string email)
    {
        var req = Valid(); req.Email = email;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    // ── Password ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Short1")]      // 6 chars < min 8
    [InlineData("nouppercase1")]  // no uppercase
    [InlineData("NOLOWERCASE1")]  // no lowercase
    [InlineData("NoDigitPass")]   // no digit
    public void Invalid_Password_Fails(string password)
    {
        var req = Valid(); req.Password = password;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidPassword_NoSpecialChar_Passes()
    {
        // CreateUser validator does NOT require special chars (unlike ChangePassword)
        var req = Valid(); req.Password = "StrongPass1";
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    // ── Role ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(UserRole.SuperAdmin)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.TenantAdmin)]
    [InlineData(UserRole.BranchManager)]
    [InlineData(UserRole.BranchUser)]
    public void All_ValidRoles_Pass(UserRole role)
    {
        var req = Valid(); req.Role = role;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void OutOfRange_Role_Fails()
    {
        var req = Valid(); req.Role = (UserRole)999;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Role);
    }
}
