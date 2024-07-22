using FC.Codeflix.Catalog.Application.UseCases.Category.CreateCategory;
using FC.Codeflix.Catalog.UnitTests.Application.Category.Common;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTestFixtureCollection : ICollectionFixture<CreateCategoryTestFixture>
{
    
}

public class CreateCategoryTestFixture : CategoriesUseCaseBaseFixture
{
    public CreateCategoryTestFixture() : base() {}

    
    public CreateCategoryInput GetInput() =>
        new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    
    public CreateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetInput();
        invalidInputShortName.Name = invalidInputShortName.Name.Substring(0, 2);
        return invalidInputShortName;
    }
    
    public CreateCategoryInput GetInvalidInputTooLongName()
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
    
    public CreateCategoryInput GetInvalidInputDescriptionNull()
    {
        var invalidInputDescriptionNull = GetInput();
        invalidInputDescriptionNull.Description = null;
        return invalidInputDescriptionNull;
    }
    
    public CreateCategoryInput GetInvalidInputTooLongDescription()
    {
        CreateCategoryTestFixture fixture;
        var invalidInputTooLongDescription = GetInput();
        var tooLongDescription = "";
        while (tooLongDescription.Length <= 10_000)
        {
            tooLongDescription = $"{tooLongDescription} {GetValidCategoryDescription()}";
        }
        invalidInputTooLongDescription.Description = tooLongDescription;
        return invalidInputTooLongDescription;
    }
    
    public CreateCategoryInput InvalidInputNameNull()
    {
        var invalidInputNameNull = GetInput();
        invalidInputNameNull.Name = null!;
        return invalidInputNameNull;
    }

}