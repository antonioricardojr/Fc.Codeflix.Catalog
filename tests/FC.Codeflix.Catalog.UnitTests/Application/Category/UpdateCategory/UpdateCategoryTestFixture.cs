using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using FC.Codeflix.Catalog.Domain.Repository;
using FC.Codeflix.Catalog.UnitTests.Application.Category.Common;
using FC.Codeflix.Catalog.UnitTests.Common;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.UpdateCategory;

[CollectionDefinition(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTestFixtureCollection : ICollectionFixture<UpdateCategoryTestFixture>
{
    
}

public class UpdateCategoryTestFixture : CategoriesUseCaseBaseFixture
{
    public UpdateCategoryTestFixture() : base()
    {
    }
    
    public string GetValidCategoryName()
    {
        var categoryName = "";
        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];
        if (categoryName.Length > 255)
            categoryName = categoryName[..255];
        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();

        if (categoryDescription.Length > 10_000)
        {
            categoryDescription = categoryDescription[.. 10_000];
        }
        return categoryDescription;
    }

    public bool GetRandomBoolean()
    {
        return (new Random()).NextDouble() < 0.5;
    }

    public Catalog.Domain.Entity.Category GetValidCategory() => 
        new (GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

    public UpdateCategoryInput GetValidInput(Guid? id = null)
        => new(id ?? Guid.NewGuid(),
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean());
    
    public UpdateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetValidInput();
        invalidInputShortName.Name = invalidInputShortName.Name.Substring(0, 2);
        return invalidInputShortName;
    }
    
    public UpdateCategoryInput GetInvalidInputTooLongName()
    {
        var invalidInputTooLongName = GetValidInput();
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
        var invalidInputDescriptionNull = GetValidInput();
        invalidInputDescriptionNull.Description = null;
        return invalidInputDescriptionNull;
    }
    
    public UpdateCategoryInput GetInvalidInputTooLongDescription()
    {
        UpdateCategoryTestFixture fixture;
        var invalidInputTooLongDescription = GetValidInput();
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
        var invalidInputNameNull = GetValidInput();
        invalidInputNameNull.Name = null!;
        return invalidInputNameNull;
    }
}