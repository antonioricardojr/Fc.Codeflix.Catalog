using EndToEndTests.Api.Genre.Common;
using Xunit;

namespace EndToEndTests.Api.Genre.GetGenre;

[CollectionDefinition(nameof(GetGenreApiTestFixture))]
public class GetGenreApiTestFixtureCollection : ICollectionFixture<GetGenreApiTestFixture>
{
    
}
public class GetGenreApiTestFixture : GenreBaseFixture
{

}