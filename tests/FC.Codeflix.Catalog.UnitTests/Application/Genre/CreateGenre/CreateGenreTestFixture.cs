using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;
using FC.Codeflix.Catalog.Domain.Repository;
using FC.Codeflix.Catalog.UnitTests.Application.Genre.Common;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.CreateGenre;


[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection : ICollectionFixture<CreateGenreTestFixture>
{
    
}

public class CreateGenreTestFixture : GenreUseCaseBaseFixture
{
    public CreateGenreInput GetExampleInput()
    {
        return new CreateGenreInput(GetValidGenreName(), GetRandomBoolean());
    }
    public CreateGenreInput GetExampleInput(string? name)
    {
        return new CreateGenreInput(name, GetRandomBoolean());
    }

    public Mock<IGenreRepository> GetGenreRepositoryMock() => new();
    public Mock<IUnitOfWork> GetUnitOfWorkMock() => new();
    public Mock<ICategoryRepository> GetCategoryRepositoryMock() => new();
    public CreateGenreInput GetExampleInputWithCategories()
    {
        var numberOfCategoryIds = new Random().Next(1, 10);

        var categoryIds = Enumerable.Range(0, numberOfCategoryIds).Select(_ => Guid.NewGuid()).ToList();
        
        
        return new CreateGenreInput(GetValidGenreName(), GetRandomBoolean(), categoryIds);
    }
    

}