using System.Net;
using EndToEndTests.Models;
using FC.Codeflix.Catalog.Application.UseCases.Category.Common;
using FC.Codeflix.Catalog.Application.UseCases.Category.ListCategories;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EndToEndTests.Api.Category.ListCategories;

class Meta
{
    public Meta(int currentPage, int perPage, int total)
    {
        CurrentPage = currentPage;
        PerPage = perPage;
        Total = total;
    }

    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
}
[Collection(nameof(ListCategoriesApiTestFixture))]
public class ListCategoriesApiTest : IDisposable
{
    private readonly ListCategoriesApiTestFixture _fixture;

    public ListCategoriesApiTest(ListCategoriesApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(ListCategoriesAndTotalByDefault))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotalByDefault()
    {
        // arrange
        var defaultPerPage = 15;
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);

        // act
        var (response, output) = await _fixture.ApiClient.Get<TestApiResponseList<CategoryModelOutput>>("/categories");

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output!.Meta.Total.Should().Be(exampleCategoriesList.Count);
        output!.Data.Should().HaveCount(defaultPerPage);
        foreach (var item in output.Data)
        {
            var exampleCategory = exampleCategoriesList.FirstOrDefault(i => i.Id == item.Id);
            
            item!.Name.Should().Be(exampleCategory!.Name);
            item!.Description.Should().Be(exampleCategory.Description);
            item!.IsActive.Should().Be(exampleCategory.IsActive);
            item!.CreatedAt.Should().Be(exampleCategory.CreatedAt);
        }

    }
    
    [Fact(DisplayName = nameof(ItemsEmptyWhenPersistenceEmpty))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ItemsEmptyWhenPersistenceEmpty()
    {
        // arrange

        // act
        var (response, output) = await _fixture.ApiClient.Get<TestApiResponseList<CategoryModelOutput>>("/categories");

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output!.Meta.Total.Should().Be(0);
        output!.Data.Should().HaveCount(0);
    }
    
    [Fact(DisplayName = nameof(ListCategoriesAndTotal))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotal()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var input = new ListCategoriesInput(page: 1, perPage: 5);
        // act
        var (response, output) = await _fixture.ApiClient.Get<TestApiResponseList<CategoryModelOutput>>($"/categories?page={input.Page}&per_page={input.PerPage}");

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output!.Meta.Total.Should().Be(exampleCategoriesList.Count);
        output!.Data.Should().HaveCount(input.PerPage);
        foreach (var item in output.Data)
        {
            var exampleCategory = exampleCategoriesList.FirstOrDefault(i => i.Id == item.Id);
            
            item!.Name.Should().Be(exampleCategory!.Name);
            item!.Description.Should().Be(exampleCategory.Description);
            item!.IsActive.Should().Be(exampleCategory.IsActive);
            item!.CreatedAt.Should().Be(exampleCategory.CreatedAt);
        }

    }
    
    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(int numCategoriesToGenerate, int page, int perPage, int expectedNumItems)
    {
        var exampleCategoriesList = _fixture.GetExampleCategoryList(numCategoriesToGenerate);
        
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var input = new ListCategoriesInput(page: page, perPage: perPage);
        // act
        var (response, output) = await _fixture.ApiClient.Get<TestApiResponseList<CategoryModelOutput>>($"/categories?page={input.Page}&per_page={input.PerPage}");
        
        

        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        output.Meta.Total.Should().Be(numCategoriesToGenerate);
        output.Data.Should().HaveCount(expectedNumItems);
        foreach (var outputItem in output.Data)
        {
            var category = exampleCategoriesList.Find(category => category.Id == outputItem.Id);

            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(category!.Id);
            outputItem.Name.Should().Be(category.Name);
            outputItem.Description.Should().Be(category.Description);
            outputItem.CreatedAt.Should().Be(category.CreatedAt);
            outputItem.IsActive.Should().Be(category.IsActive);
        }
    }
    
    [Theory(DisplayName = nameof(ListByText))]
    [Trait("EndToEnd/API", "Category/List - Endpoints")]
    [InlineData("Action", 1, 5, 1,  1)]
    [InlineData("Horror", 1, 5, 3,  3)]
    [InlineData("Horror", 2, 5, 0,  3)]
    [InlineData("Sci-Fi", 1, 5, 4,  4)]
    [InlineData("Sci-Fi", 1, 2, 2,  4)]
    [InlineData("Sci-Fi", 2, 3, 1,  4)]
    [InlineData("Sci-Fi Other", 1, 5, 0,  0)]
    [InlineData("Robots", 1, 5, 2,  2)]
    public async Task ListByText(string search, int page, int perPage, int expectedNumItemsReturned, int expectedNumTotalItems)
    {

        var categoryNamesList = new List<string>()
        {
            "Action", "Horror", "Horror Robots", "Horror - Based on Real Facts", "Drama", "Sci-Fi", "Sci-Fi AI",
            "Sci-Fi Space", "Sci-Fi Robots"
        };
        
        var exampleCategoriesList = _fixture.GetExampleCategoriesListWithNames(categoryNamesList);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        
        // act
        var (response, output) = await _fixture.ApiClient.Get<TestApiResponseList<CategoryModelOutput>>($"/categories?page={page}&per_page={perPage}&search={search}");


        output.Should().NotBeNull();
        output!.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output!.Meta.CurrentPage.Should().Be(page);
        output.Meta.PerPage.Should().Be(perPage);
        output.Meta.Total.Should().Be(expectedNumTotalItems);
        output.Data.Should().HaveCount(expectedNumItemsReturned);
        foreach (var outputItem in output.Data)
        {
            var category = exampleCategoriesList.Find(category => category.Id == outputItem.Id);

            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(category!.Id);
            outputItem.Name.Should().Be(category.Name);
            outputItem.Description.Should().Be(category.Description);
            outputItem.CreatedAt.Should().Be(category.CreatedAt);
            outputItem.IsActive.Should().Be(category.IsActive);
        }
    }
    
    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("Integration/Application", "List Categories - UseCases")]
    [InlineData("name","asc")]
    [InlineData("name","desc")]
    [InlineData("id","asc")]
    [InlineData("id","desc")]
    [InlineData("createdAt","asc")]
    [InlineData("createdAt","desc")]
    public async Task ListOrdered(string orderBy, string order)
    {
        var exampleCategoriesList = _fixture.GetExampleCategoryList(10);
        
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        
        var searchOrder = order.ToLower() == "desc" ? SearchOrder.Desc : SearchOrder.Asc;
        
        // act
        var (response, output) = await _fixture.ApiClient.Get<TestApiResponseList<CategoryModelOutput>>($"/categories?sort={orderBy}&order={searchOrder}");

        var expectedOrderedList = _fixture.CloneCategoriesListOrdered(exampleCategoriesList, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Meta.Should().NotBeNull();
        output.Data.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output.Meta.Total.Should().Be(exampleCategoriesList.Count);
        output.Data.Should().HaveCount(exampleCategoriesList.Count);

        for (int i = 0; i < expectedOrderedList.Count; i++)
        {
            var expectedItem = expectedOrderedList[i];
            var outputItem = output.Data[i];
            outputItem.Should().NotBeNull();
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.CreatedAt.Should().Be(expectedItem.CreatedAt);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
        }
    }

    public void Dispose() => _fixture.CleanPersistence();
}