using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Application.UseCases.Genre.UpdateGenre;
using FC.Codeflix.Catalog.Domain.Repository;
using FC.Codeflix.Catalog.UnitTests.Application.Genre.Common;
using FC.Codeflix.Catalog.UnitTests.Domain.Entity.Genre;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Genre.UpdateGenre;


[CollectionDefinition(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTestFixtureCollection : ICollectionFixture<UpdateGenreTestFixture>
{
    
}

public class UpdateGenreTestFixture : GenreUseCaseBaseFixture
{
    public UpdateGenreInput GetExampleInput(string? name)
    {
        return new UpdateGenreInput(Guid.NewGuid(),name, GetRandomBoolean());
    }
}