using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Domain.Repository;
using FC.Codeflix.Catalog.UnitTests.Common;
using Moq;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCaseBaseFixture : BaseFixture
{
    public string GetValidGenreName()
    {
        return Faker.Commerce.Categories(1)[0];
    }

    public Catalog.Domain.Entity.Genre GetExampleGenre(bool isActive = true, List<Guid>? categoryIds = null)
    {
        
        var genre = new Catalog.Domain.Entity.Genre(GetValidGenreName(), isActive);
        if (categoryIds is not null)
        {
            categoryIds.ForEach(id => genre.AddCategory(id));
        }
        return genre;
    }
    
    public List<Catalog.Domain.Entity.Genre> GetExampleGenreList(int count = 10)
    {
        return Enumerable.Range(1, count).Select(_ => GetExampleGenre(categoryIds: GetRandomIdsList())).ToList();
    }
    public List<Guid> GetRandomIdsList(int? count = null)
    {
        return Enumerable.Range(0, count ?? (new Random()).Next(1, 10)).Select(_ => Guid.NewGuid()).ToList();
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
        var categoryDescription =
            Faker.Commerce.ProductDescription();
        if (categoryDescription.Length > 10_000)
            categoryDescription =
                categoryDescription[..10_000];
        return categoryDescription;
    }

    public Catalog.Domain.Entity.Category GetExampleCategory()
        => new(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );

    public List<Catalog.Domain.Entity.Category> GetExampleCategoriesList(int count = 5)
        => Enumerable.Range(0, count).Select(_ => GetExampleCategory())
            .ToList();
    
    public Mock<IGenreRepository> GetGenreRepositoryMock() => new();
    public Mock<IUnitOfWork> GetUnitOfWorkMock() => new();
    public Mock<ICategoryRepository> GetCategoryRepositoryMock() => new();
}