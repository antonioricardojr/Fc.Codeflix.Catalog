using FC.Codeflix.Catalog.UnitTests.Common;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Domain.Entity.Genre;

[CollectionDefinition(nameof(GenreTestFixture))]
public class GenreTestFixtureCollection : ICollectionFixture<GenreTestFixture> {

}

public class GenreTestFixture : BaseFixture
{
    public string GetValidName() => Faker.Commerce.Categories(1)[0];

    public Catalog.Domain.Entity.Genre GetExampleGenre(bool isActive = true, List<Guid>? categoryIds = null)
    {
        
        var genre = new Catalog.Domain.Entity.Genre(GetValidName(), isActive);
        if (categoryIds is not null)
        {
            categoryIds.ForEach(id => genre.AddCategory(id));
        }
        return genre;
    }
}