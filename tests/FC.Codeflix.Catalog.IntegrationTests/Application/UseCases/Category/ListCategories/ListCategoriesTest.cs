using FC.Codeflix.Catalog.Application.UseCases.Category.ListCategories;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[Collection(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTest
{
    private readonly ListCategoriesTestFixture _fixture;

    public ListCategoriesTest(ListCategoriesTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(DisplayName = nameof(SearchReturnsListAndTotal))]
    [Trait("Integration/Application", "ListCategories - Repositories")]
    public async Task SearchReturnsListAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(15);
        await dbContext.AddRangeAsync(exampleCategorisList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new ListCategoriesInput(1, 20);
        var useCase = new Catalog.Application.UseCases.Category.ListCategories.ListCategories(categoryRepository);
        
        var output = await useCase.Handle(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(output.Items.Count);
        output.Items.Should().HaveCount(exampleCategorisList.Count);
        foreach (var outputItem in output.Items)
        {
            var category = exampleCategorisList.Find(category => category.Id == outputItem.Id);

            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(category.Id);
            outputItem.Name.Should().Be(category.Name);
            outputItem.Description.Should().Be(category.Description);
            outputItem.CreatedAt.Should().Be(category.CreatedAt);
            outputItem.IsActive.Should().Be(category.IsActive);
        }
    }
    
    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenEmpty))]
    [Trait("Integration/Application", "ListCategories - Repositories")]
    public async Task SearchReturnsEmptyWhenEmpty()
    {
        var dbContext = _fixture.CreateDbContext();
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new ListCategoriesInput(1, 20);
        var useCase = new Catalog.Application.UseCases.Category.ListCategories.ListCategories(categoryRepository);
        
        var output = await useCase.Handle(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(output.Items.Count);
        output.Items.Should().HaveCount(0);
    }
    
    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchReturnsPaginated(int numCategoriesToGenerate, int page, int perPage, int expectedNumItems)
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(numCategoriesToGenerate);
        
        await dbContext.AddRangeAsync(exampleCategorisList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new ListCategoriesInput(page, perPage);
        var useCase = new Catalog.Application.UseCases.Category.ListCategories.ListCategories(categoryRepository);
        
        var output = await useCase.Handle(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(numCategoriesToGenerate);
        output.Items.Should().HaveCount(expectedNumItems);
        foreach (var outputItem in output.Items)
        {
            var category = exampleCategorisList.Find(category => category.Id == outputItem.Id);

            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(category.Id);
            outputItem.Name.Should().Be(category.Name);
            outputItem.Description.Should().Be(category.Description);
            outputItem.CreatedAt.Should().Be(category.CreatedAt);
            outputItem.IsActive.Should().Be(category.IsActive);
        }
    }
    
    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListCategories - UseCases")]
    [InlineData("Action", 1, 5, 1,  1)]
    [InlineData("Horror", 1, 5, 3,  3)]
    [InlineData("Horror", 2, 5, 0,  3)]
    [InlineData("Sci-Fi", 1, 5, 4,  4)]
    [InlineData("Sci-Fi", 1, 2, 2,  4)]
    [InlineData("Sci-Fi", 2, 3, 1,  4)]
    [InlineData("Sci-Fi Other", 1, 5, 0,  0)]
    [InlineData("Robots", 1, 5, 2,  2)]
    public async Task SearchByText(string search, int page, int perPage, int expectedNumItemsReturned, int expectedNumTotalItems)
    {

        var categoryNamesList = new List<string>()
        {
            "Action", "Horror", "Horror Robots", "Horror - Based on Real Facts", "Drama", "Sci-Fi", "Sci-Fi AI",
            "Sci-Fi Space", "Sci-Fi Robots"
        };
        
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesListWithNames(categoryNamesList);
        
        await dbContext.AddRangeAsync(exampleCategorisList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new ListCategoriesInput(page, perPage, search, "", SearchOrder.Asc);

        var useCase = new Catalog.Application.UseCases.Category.ListCategories.ListCategories(categoryRepository);
        
        var output = await useCase.Handle(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(expectedNumTotalItems);
        output.Items.Should().HaveCount(expectedNumItemsReturned);
        foreach (var outputItem in output.Items)
        {
            var category = exampleCategorisList.Find(category => category.Id == outputItem.Id);

            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(category.Id);
            outputItem.Name.Should().Be(category.Name);
            outputItem.Description.Should().Be(category.Description);
            outputItem.CreatedAt.Should().Be(category.CreatedAt);
            outputItem.IsActive.Should().Be(category.IsActive);
        }
    }
    
    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Application", "List Categories - UseCases")]
    [InlineData("name","asc")]
    [InlineData("name","desc")]
    [InlineData("id","asc")]
    [InlineData("id","desc")]
    [InlineData("createdAt","asc")]
    [InlineData("createdAt","desc")]
    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(10);
        
        await dbContext.AddRangeAsync(exampleCategorisList!);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchOrder = order.ToLower() == "desc" ? SearchOrder.Desc : SearchOrder.Asc;
        var searchInput = new SearchInput(1, 20, "", orderBy, searchOrder);
        
        
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(exampleCategorisList, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(exampleCategorisList.Count);
        output.Items.Should().HaveCount(exampleCategorisList.Count);

        for (int i = 0; i < expectedOrderedList.Count; i++)
        {
            var expectedItem = expectedOrderedList[i];
            var outputItem = output.Items[i];
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
        }
    }
}