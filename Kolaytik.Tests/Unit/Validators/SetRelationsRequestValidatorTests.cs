using FluentValidation.TestHelper;
using Kolaytik.API.Validators;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.Tests.Unit.Validators;

public class SetRelationsRequestValidatorTests
{
    private readonly SetRelationsRequestValidator _validator = new();

    [Fact]
    public void Empty_List_Passes()
    {
        var result = _validator.TestValidate(new SetRelationsRequest { ChildItemIds = [] });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Unique_Ids_Pass()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var result = _validator.TestValidate(new SetRelationsRequest { ChildItemIds = ids });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SingleItem_List_Passes()
    {
        var result = _validator.TestValidate(new SetRelationsRequest { ChildItemIds = [Guid.NewGuid()] });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Duplicate_Ids_Fail()
    {
        var id = Guid.NewGuid();
        var result = _validator.TestValidate(new SetRelationsRequest
        {
            ChildItemIds = [id, Guid.NewGuid(), id]
        });
        result.ShouldHaveValidationErrorFor(x => x.ChildItemIds);
    }

    [Fact]
    public void AllDuplicates_Fail()
    {
        var id = Guid.NewGuid();
        var result = _validator.TestValidate(new SetRelationsRequest
        {
            ChildItemIds = [id, id, id]
        });
        result.ShouldHaveValidationErrorFor(x => x.ChildItemIds);
    }
}
