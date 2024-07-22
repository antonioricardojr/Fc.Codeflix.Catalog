using System.Net;
using FC.Codeflix.Catalog.Application.UseCases.Category.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EndToEndTests.Api.Category.GetCategory;

class GetCategoryResponse
{
    public CategoryModelOutput Data { get; set;  }
    public GetCategoryResponse(CategoryModelOutput data)
    {
        Data = data;
    }
}

[Collection(nameof(GetCategoryApiTestFixture))]
public class GetCategoryApiTest : IDisposable
{
    private readonly GetCategoryApiTestFixture _fixture;

    public GetCategoryApiTest(GetCategoryApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(GetCategory))]
    [Trait("EndToEnd/API", "Category/Get - Endpoints")]
    public async Task GetCategory()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var exampleCategory = exampleCategoriesList[10];

        // act
        var (response, output) = await _fixture.ApiClient.Get<GetCategoryResponse>($"/categories/{exampleCategory.Id}");

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output!.Data.Id.Should().Be(exampleCategory.Id);
        output!.Data.Name.Should().Be(exampleCategory.Name);
        output!.Data.Description.Should().Be(exampleCategory.Description);
        output!.Data.IsActive.Should().Be(exampleCategory.IsActive);
        output!.Data.CreatedAt.Should().Be(exampleCategory.CreatedAt);
    }
    
    [Fact(DisplayName = nameof(ErrorWhenNotFound))]
    [Trait("EndToEnd/API", "Category/Get - Endpoints")]
    public async Task ErrorWhenNotFound()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var randomId = Guid.NewGuid();

        // act
        var (response, output) = await _fixture.ApiClient.Get<ProblemDetails>($"/categories/{randomId}");

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status404NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status404NotFound);
        output!.Title.Should().Be("Not Found");
        output!.Detail.Should().Be($"Category '{randomId}' not found.");
        output.Type.Should().Be("NotFound");

    }
    
    public void Dispose() => _fixture.CleanPersistence();
    
}