using EndToEndTests.Base;

namespace EndToEndTests.Api.Category.Common;

public class CategoryBaseFixture : BaseFixture
{
    public CategoryPersistence Persistence;

    public CategoryBaseFixture() : base()
    {
        Persistence = new CategoryPersistence(CreateDbContext());
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
    public bool GetRandomBoolean() => new Random().NextDouble() < 0.5;
    
    public string GetInvalidNametooShort()
    {
        return Faker.Commerce.ProductName().Substring(0, 2);
    }
    
    public string GetInvalidNameTooLong()
    {
        var tooLongName = "";
        while (tooLongName.Length <= 255)
        {
            tooLongName = $"{tooLongName} {GetValidCategoryName()}";
        }

        return tooLongName;
    }
    
    public string GetInvalidInputTooLongDescription()
    {
        var tooLongDescription = "";
        while (tooLongDescription.Length <= 10_000)
        {
            tooLongDescription = $"{tooLongDescription} {GetValidCategoryDescription()}";
        }
        return tooLongDescription;
    }

    public FC.Codeflix.Catalog.Domain.Entity.Category GetExampleCategory()
    {
        return new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    }

    public List<FC.Codeflix.Catalog.Domain.Entity.Category> GetExampleCategoryList(int listLength)
    {
        return Enumerable.Range(1, listLength).Select(_ => GetExampleCategory()).ToList();
    }
}