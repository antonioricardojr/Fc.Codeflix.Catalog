using FC.Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[CollectionDefinition(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTestFixtureCollection : ICollectionFixture<UpdateCategoryTestFixture>
{
    
}

public class UpdateCategoryTestFixture : CategoryUseCasesBaseFixture
{
    public UpdateCategoryInput GetInput() =>
        new( Guid.NewGuid() ,GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    
    public UpdateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetInput();
        invalidInputShortName.Name = invalidInputShortName.Name.Substring(0, 2);
        return invalidInputShortName;
    }
    
    public UpdateCategoryInput GetInvalidInputTooLongName()
    {
        var invalidInputTooLongName = GetInput();
        var tooLongName = "";
        while (tooLongName.Length <= 255)
        {
            tooLongName = $"{tooLongName} {GetValidCategoryName()}";
        }

        invalidInputTooLongName.Name = tooLongName;
        return invalidInputTooLongName;
    }
    
    public UpdateCategoryInput GetInvalidInputDescriptionNull()
    {
        var invalidInputDescriptionNull = GetInput();
        invalidInputDescriptionNull.Description = null;
        return invalidInputDescriptionNull;
    }
    
    public UpdateCategoryInput GetInvalidInputTooLongDescription()
    {
        UpdateCategoryTestFixture fixture;
        var invalidInputTooLongDescription = GetInput();
        var tooLongDescription = "";
        while (tooLongDescription.Length <= 10_000)
        {
            tooLongDescription = $"{tooLongDescription} {GetValidCategoryDescription()}";
        }
        invalidInputTooLongDescription.Description = tooLongDescription;
        return invalidInputTooLongDescription;
    }
    
    public UpdateCategoryInput InvalidInputNameNull()
    {
        var invalidInputNameNull = GetInput();
        invalidInputNameNull.Name = null!;
        return invalidInputNameNull;
    }
    
    public UpdateCategoryInput GetValidInput(Guid? id = null)
        => new(
            id ?? Guid.NewGuid(),
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );
}