using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FC.Codeflix.Catalog.IntegrationTests.Base;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;

public class GenreUseCasesBaseFixture : BaseFixture
{
    public string GetValidGenreName()
    {
        var categoryName = "";
        while (categoryName.Length < 3)
            categoryName = Faker.Commerce.Categories(1)[0];
        if (categoryName.Length > 255)
            categoryName = categoryName[..255];
        return categoryName;
    }
    public bool GetRandomBoolean() => new Random().NextDouble() < 0.5;
    public Domain.Entity.Genre? GetExampleGenre(string? name = null) => 
        new (name ?? GetValidGenreName(), GetRandomBoolean());

    public List<Domain.Entity.Genre?> GetExampleListGenres(int count = 10) =>
        Enumerable.Range(1, count).Select(_ => GetExampleGenre()).ToList();
    
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
    
    public Domain.Entity.Category? GetExampleCategory() => 
        new (GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    
    public List<Domain.Entity.Category?> GetExampleCategoriesList(int length = 10) =>
        Enumerable.Range(1, length).Select(_ => GetExampleCategory()).ToList();
}