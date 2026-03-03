using FluentValidation.TestHelper;
using Kolaytik.API.Validators;
using Kolaytik.Core.DTOs.Auth;

namespace Kolaytik.Tests.Unit.Validators;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    private static ChangePasswordRequest Valid() => new()
    {
        CurrentPassword = "OldP@ss1",
        NewPassword     = "NewP@ss1"
    };

    [Fact]
    public void Valid_Request_Passes()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    // ── CurrentPassword ───────────────────────────────────────────────────────

    [Fact]
    public void Empty_CurrentPassword_Fails()
    {
        var req = Valid(); req.CurrentPassword = "";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    // ── NewPassword length ────────────────────────────────────────────────────

    [Fact]
    public void TooShort_NewPassword_Fails()
    {
        var req = Valid(); req.NewPassword = "Ab1!";   // 4 chars
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void AtMinLength_NewPassword_Passes()
    {
        var req = Valid(); req.NewPassword = "Ab1!ab1!";  // exactly 8 chars
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    // ── Character class requirements ──────────────────────────────────────────

    [Fact]
    public void NoUppercase_Fails()
    {
        var req = Valid(); req.NewPassword = "newp@ss1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NoLowercase_Fails()
    {
        var req = Valid(); req.NewPassword = "NEWP@SS1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NoDigit_Fails()
    {
        var req = Valid(); req.NewPassword = "NewP@ssword";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NoSpecialChar_Fails()
    {
        var req = Valid(); req.NewPassword = "NewPass1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    // ── Same-as-current rule ──────────────────────────────────────────────────

    [Fact]
    public void SameAsCurrentPassword_Fails()
    {
        var req = new ChangePasswordRequest
        {
            CurrentPassword = "SameP@ss1",
            NewPassword     = "SameP@ss1"
        };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void DifferentFromCurrent_Passes()
    {
        var req = new ChangePasswordRequest
        {
            CurrentPassword = "OldP@ss1",
            NewPassword     = "NewP@ss1"
        };
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }
}
