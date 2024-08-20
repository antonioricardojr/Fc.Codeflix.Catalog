using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Category.ListCategories;
using FC.Codeflix.Catalog.Domain.Entity;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CategoryRepository;

[Collection(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTest
{
    private readonly CategoryRepositoryTestFixture _fixture;

    public CategoryRepositoryTest(CategoryRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();

        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        await categoryRepository.Insert(exampleCategory, CancellationToken.None);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbCategory = await dbContext.Categories.FindAsync(exampleCategory.Id);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(exampleCategory.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }
    
    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Get()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(5);
        exampleCategorisList.Add(exampleCategory);
        await dbContext.AddRangeAsync(exampleCategorisList!);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(_fixture.CreateDbContext(true));

        var dbCategory = await categoryRepository.Get(exampleCategory!.Id, CancellationToken.None);
        
        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(exampleCategory.Name);
        dbCategory.Description.Should().Be(exampleCategory.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }
    
    [Fact(DisplayName = nameof(GetThrowsIfNotFound))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task GetThrowsIfNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(5);
        await dbContext.AddRangeAsync(exampleCategorisList!);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(_fixture.CreateDbContext());

        var action = async () => await categoryRepository.Get(exampleCategory!.Id, CancellationToken.None);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Category '{exampleCategory!.Id}' not found.");

    }
    
    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Update()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var newCategoryValues = _fixture.GetExampleCategory();
        
        var exampleCategorisList = _fixture.GetExampleCategoriesList(5);
        exampleCategorisList.Add(exampleCategory);
        await dbContext.AddRangeAsync(exampleCategorisList!);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(_fixture.CreateDbContext(true));

        exampleCategory!.Update(newCategoryValues!.Name, newCategoryValues.Description);
        
        await categoryRepository.Update(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbCategory = await categoryRepository.Get(exampleCategory!.Id, CancellationToken.None);

        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be(newCategoryValues.Name);
        dbCategory.Description.Should().Be(newCategoryValues.Description);
        dbCategory.IsActive.Should().Be(exampleCategory.IsActive);
        dbCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }
    
    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Delete()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(5);
        exampleCategorisList.Add(exampleCategory);
        
        await dbContext.AddRangeAsync(exampleCategorisList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);
        
        await categoryRepository.Delete(exampleCategory, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbCategory = await (_fixture.CreateDbContext())
            .Categories.FindAsync(exampleCategory!.Id);

        dbCategory.Should().BeNull();
    }
    
    [Fact(DisplayName = nameof(SearchReturnsListAndTotal))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsListAndTotal()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(15);
        
        await dbContext.AddRangeAsync(exampleCategorisList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);
        
        
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
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
    
    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenPersistenceEmpty))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsEmptyWhenPersistenceEmpty()
    {
        var dbContext = _fixture.CreateDbContext();
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);
        
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.PerPage.Should().Be(searchInput.PerPage);
        output.Total.Should().Be(0);
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

        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.Asc);
        
        
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
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
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesListWithNames(new ()
        {
            "Action", "Horror","Horror Robots",  "Horror - Based on Real Facts", "Drama", "Sci-Fi", "Sci-Fi AI", "Sci-Fi Space", "Sci-Fi Robots"
        });
        
        await dbContext.AddRangeAsync(exampleCategorisList);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.Asc);
        
        
        var output = await categoryRepository.Search(searchInput, CancellationToken.None);
        

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
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
    [Trait("Integration/Application", "CategoryRepository - Repositories")]
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
        var searchInput = new ListCategoriesInput(1, 20, "", orderBy, searchOrder);
        var useCase = new ListCategories(categoryRepository);
        
        var output = await useCase.Handle(searchInput, CancellationToken.None);

        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(exampleCategorisList, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(searchInput.Page);
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
    
    [Fact(DisplayName = nameof(ListByIds))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task ListByIds()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategorisList = _fixture.GetExampleCategoriesList(15);
        List<Guid> categoryIdsToGet = Enumerable.Range(1, 3).Select(_ =>
        {
            int indexToGet = (new Random()).Next(0, exampleCategorisList.Count - 1);
            return exampleCategorisList[indexToGet]!.Id;
        }).Distinct().ToList();
        await dbContext.AddRangeAsync(exampleCategorisList!);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        
        var categoryRepository = new Catalog.Infra.Data.EF.Repositories
            .CategoryRepository(dbContext);

        IReadOnlyList<Category> categoriesListOutput = await categoryRepository.ListByIds(categoryIdsToGet, CancellationToken.None);


        categoryIdsToGet.Should().NotBeNull();
        categoryIdsToGet.Should().HaveCount(categoriesListOutput.Count);

        foreach (var outputItem in categoriesListOutput)
        {
            var exampleItem = exampleCategorisList.Find(category => category!.Id == outputItem.Id);
            exampleItem.Should().NotBeNull();
            outputItem.Name.Should().Be(exampleItem!.Name);
            outputItem.Description.Should().Be(exampleItem.Description);
            outputItem.IsActive.Should().Be(exampleItem.IsActive);
            outputItem.CreatedAt.Should().Be(exampleItem.CreatedAt);
        }
    }

}