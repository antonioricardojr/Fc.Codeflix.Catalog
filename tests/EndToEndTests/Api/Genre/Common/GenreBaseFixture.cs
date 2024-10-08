using EndToEndTests.Api.Category.Common;
using EndToEndTests.Base;

namespace EndToEndTests.Api.Genre.Common;

public class GenreBaseFixture : BaseFixture
{
    public GenrePersistence Persistence;
    public CategoryPersistence CategoryPersistence;

    public GenreBaseFixture() : base()
    {
        var dbContext = CreateDbContext();
        Persistence = new GenrePersistence(dbContext);
        CategoryPersistence = new CategoryPersistence(dbContext);
    }

    public string GetValidGenreName()
    {
        var genreName = "";
        while (genreName.Length < 3)
            genreName = Faker.Commerce.Categories(1)[0];
        if (genreName.Length > 255)
            genreName = genreName[..255];
        return genreName;
    }
    public bool GetRandomBoolean() => new Random().NextDouble() < 0.5;

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
    
    public List<FC.Codeflix.Catalog.Domain.Entity.Genre> GetExampleGenresList(int listLength)
    {
        return Enumerable.Range(1, listLength).Select(_ => GetExampleGenre()).ToList();
    }

    private FC.Codeflix.Catalog.Domain.Entity.Genre GetExampleGenre()
    {
        return new(GetValidGenreName(), GetRandomBoolean());
    }
    
    public FC.Codeflix.Catalog.Domain.Entity.Category? GetExampleCategory() => 
        new (GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    
    public List<FC.Codeflix.Catalog.Domain.Entity.Category?> GetExampleCategoriesList(int length = 10) =>
        Enumerable.Range(1, length).Select(_ => GetExampleCategory()).ToList();
}