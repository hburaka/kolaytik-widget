using FluentValidation.TestHelper;
using Kolaytik.API.Validators;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.Tests.Unit.Validators;

public class CreateListRequestValidatorTests
{
    private readonly CreateListRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var result = _validator.TestValidate(new CreateListRequest
        {
            Name        = "Çalışanlar",
            Description = "Firma çalışan listesi"
        });
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Name ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("A")]   // 1 char — below min 2
    public void TooShort_Name_Fails(string name)
    {
        var result = _validator.TestValidate(new CreateListRequest { Name = name });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void TwoChar_Name_Passes()
    {
        var result = _validator.TestValidate(new CreateListRequest { Name = "AB" });
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void TooLong_Name_Fails()
    {
        var result = _validator.TestValidate(new CreateListRequest { Name = new string('A', 201) });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void MaxLength_Name_Passes()
    {
        var result = _validator.TestValidate(new CreateListRequest { Name = new string('A', 200) });
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── Description ───────────────────────────────────────────────────────────

    [Fact]
    public void Null_Description_Passes()
    {
        var result = _validator.TestValidate(new CreateListRequest { Name = "Valid", Description = null });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TooLong_Description_Fails()
    {
        var result = _validator.TestValidate(new CreateListRequest
        {
            Name        = "Valid",
            Description = new string('x', 1001)
        });
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void MaxLength_Description_Passes()
    {
        var result = _validator.TestValidate(new CreateListRequest
        {
            Name        = "Valid",
            Description = new string('x', 1000)
        });
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
