using FluentValidation.TestHelper;
using Kolaytik.API.Validators;
using Kolaytik.Core.DTOs.Auth;

namespace Kolaytik.Tests.Unit.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var result = _validator.TestValidate(new LoginRequest
        {
            Email    = "user@kolaytik.com",
            Password = "secret123"
        });
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public void Invalid_Email_Fails(string email)
    {
        var result = _validator.TestValidate(new LoginRequest { Email = email, Password = "secret123" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_ExceedingMaxLength_Fails()
    {
        var result = _validator.TestValidate(new LoginRequest
        {
            Email    = new string('a', 251) + "@b.com",   // 257 chars > 256
            Password = "secret123"
        });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // ── Password ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("12345")]   // 5 chars — below min 6
    public void Invalid_Password_Fails(string password)
    {
        var result = _validator.TestValidate(new LoginRequest { Email = "user@kolaytik.com", Password = password });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_AtMinLength_Passes()
    {
        var result = _validator.TestValidate(new LoginRequest { Email = "user@kolaytik.com", Password = "123456" });
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    // ── TotpCode ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("999999")]
    public void Valid_TotpCode_Passes(string code)
    {
        var result = _validator.TestValidate(new LoginRequest
        {
            Email = "user@kolaytik.com", Password = "secret123", TotpCode = code
        });
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }

    [Theory]
    [InlineData("12345")]    // 5 digits
    [InlineData("1234567")]  // 7 digits
    [InlineData("abcdef")]   // letters
    [InlineData("12 456")]   // space
    public void Invalid_TotpCode_Fails(string code)
    {
        var result = _validator.TestValidate(new LoginRequest
        {
            Email = "user@kolaytik.com", Password = "secret123", TotpCode = code
        });
        result.ShouldHaveValidationErrorFor(x => x.TotpCode);
    }

    [Fact]
    public void Null_TotpCode_IsAllowed()
    {
        var result = _validator.TestValidate(new LoginRequest
        {
            Email = "user@kolaytik.com", Password = "secret123", TotpCode = null
        });
        result.ShouldNotHaveValidationErrorFor(x => x.TotpCode);
    }
}
