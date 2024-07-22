using EndToEndTests.Api.Category.Common;
using FC.Codeflix.Catalog.Application.UseCases.Category.CreateCategory;
using Xunit;

namespace EndToEndTests.Api.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryApiTestFixture))]
public class CreateCategoryApiTestFixtureCollection : ICollectionFixture<CreateCategoryApiTestFixture>
{
    
}
public class CreateCategoryApiTestFixture : CategoryBaseFixture
{
    public CreateCategoryInput GetExampleInput() =>
        new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
}