using EndToEndTests.Api.Category.Common;
using FC.Codeflix.Catalog.Application.UseCases.Category.GetCategory;
using Xunit;

namespace EndToEndTests.Api.Category.GetCategory;

[CollectionDefinition(nameof(GetCategoryApiTestFixture))]
public class GetCategoryApiTestFixtureCollection : ICollectionFixture<GetCategoryApiTestFixture>
{
    
}

public class GetCategoryApiTestFixture : CategoryBaseFixture
{
}