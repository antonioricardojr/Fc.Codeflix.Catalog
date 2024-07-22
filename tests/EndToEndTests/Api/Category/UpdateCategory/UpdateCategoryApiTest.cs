using System.Net;
using FC.Codeflix.Catalog.Api.ApiModels.Category;
using FC.Codeflix.Catalog.Api.ApiModels.Response;
using FC.Codeflix.Catalog.Application.UseCases.Category.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EndToEndTests.Api.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryApiTestFixture))]
public class UpdateCategoryApiTest : IDisposable
{
    private readonly UpdateCategoryApiTestFixture _fixture;

    public UpdateCategoryApiTest(UpdateCategoryApiTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(DisplayName = nameof(UpdateCategory))]
    [Trait("EndToEnd/API", "Category/Update - Endpoints")]
    public async void UpdateCategory()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var exampleCategory = exampleCategoriesList[10];

        var input = _fixture.GetExampleInput();
        
        // act
        var (response, output) = await _fixture.ApiClient.Put<ApiResponse<CategoryModelOutput>>($"/categories/{exampleCategory.Id}", 
            input);

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(exampleCategory.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.Description.Should().Be(input.Description);
        output.Data.IsActive.Should().Be(input.IsActive!.Value);
        
        var persistenceCategory = await _fixture.Persistence.GetById(exampleCategory.Id);
        persistenceCategory.Should().NotBeNull();
        persistenceCategory!.Id.Should().Be(exampleCategory.Id);
        persistenceCategory.Name.Should().Be(input.Name);
        persistenceCategory.Description.Should().Be(input.Description);
        persistenceCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
        persistenceCategory.IsActive.Should().Be(input.IsActive!.Value);
    }
    
    [Fact(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("EndToEnd/API", "Category/Update - Endpoints")]
    public async void UpdateCategoryOnlyName()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var exampleCategory = exampleCategoriesList[10];

        var input = new UpdateCategoryApiInput(_fixture.GetValidCategoryName());
        
        // act
        var (response, output) = await _fixture.ApiClient.Put<ApiResponse<CategoryModelOutput>>($"/categories/{exampleCategory.Id}", input);

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(exampleCategory.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.Description.Should().Be(exampleCategory.Description);
        output.Data.IsActive.Should().Be(exampleCategory.IsActive);
        
        var persistenceCategory = await _fixture.Persistence.GetById(exampleCategory.Id);
        persistenceCategory.Should().NotBeNull();
        persistenceCategory!.Id.Should().Be(exampleCategory.Id);
        persistenceCategory.Name.Should().Be(input.Name);
        persistenceCategory.Description.Should().Be(exampleCategory.Description);
        persistenceCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
        persistenceCategory.IsActive.Should().Be(exampleCategory.IsActive);
    }
    
    [Fact(DisplayName = nameof(UpdateCategoryOnlyNameAndDescription))]
    [Trait("EndToEnd/API", "Category/Update - Endpoints")]
    public async void UpdateCategoryOnlyNameAndDescription()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var exampleCategory = exampleCategoriesList[10];

        var input = new UpdateCategoryApiInput(_fixture.GetValidCategoryName(), _fixture.GetValidCategoryDescription());
        
        // act
        var (response, output) = await _fixture.ApiClient.Put<ApiResponse<CategoryModelOutput>>($"/categories/{exampleCategory.Id}", input);

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(exampleCategory.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.Description.Should().Be(input.Description);
        output.Data.IsActive.Should().Be(exampleCategory.IsActive);
        
        var persistenceCategory = await _fixture.Persistence.GetById(exampleCategory.Id);
        persistenceCategory.Should().NotBeNull();
        persistenceCategory!.Id.Should().Be(exampleCategory.Id);
        persistenceCategory.Name.Should().Be(input.Name);
        persistenceCategory.Description.Should().Be(input.Description);
        persistenceCategory.CreatedAt.Should().Be(exampleCategory.CreatedAt);
        persistenceCategory.IsActive.Should().Be(exampleCategory.IsActive);
    }
    
    [Fact(DisplayName = nameof(ErrorWhenNotFound))]
    [Trait("EndToEnd/API", "Category/Update - Endpoints")]
    public async void ErrorWhenNotFound()
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var exampleCategory = exampleCategoriesList[10];

        var input = _fixture.GetExampleInput();
        var randomId = Guid.NewGuid();
        // act
        var (response, output) = await _fixture.ApiClient.Put<ProblemDetails>($"/categories/{randomId}", input);

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status404NotFound);
        output.Should().NotBeNull();
        output!.Title.Should().Be("Not Found");
        output.Type.Should().Be("NotFound");
        output.Status.Should().Be((int) StatusCodes.Status404NotFound);
        output.Detail.Should().Be($"Category '{randomId}' not found.");
    }
    
    [Theory(DisplayName = nameof(ErrorWhenCantInstantiateAggregate))]
    [Trait("EndToEnd/API", "Category/Update - Endpoints")]
    [MemberData(nameof(UpdateCategoryApiTestDataGenerator.GetInvalidInputs), 
        MemberType = typeof(UpdateCategoryApiTestDataGenerator))
    ]
    public async void ErrorWhenCantInstantiateAggregate(
        UpdateCategoryApiInput input,
        string expectedDetails
        )
    {
        // arrange
        var exampleCategoriesList = _fixture.GetExampleCategoryList(20);
        await _fixture.Persistence.BulkInsert(exampleCategoriesList);
        var exampleCategory = exampleCategoriesList[10];
        
        // act
        var (response, output) = await _fixture.ApiClient.Put<ProblemDetails>($"/categories/{exampleCategory.Id}", input);

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status422UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Title.Should().Be("One or more validation errors occurred");
        output.Type.Should().Be("UnprocessableEntity");
        output.Status.Should().Be((int) StatusCodes.Status422UnprocessableEntity);
        output.Detail.Should().Be(expectedDetails);
    }
    
    public void Dispose() => _fixture.CleanPersistence();
}