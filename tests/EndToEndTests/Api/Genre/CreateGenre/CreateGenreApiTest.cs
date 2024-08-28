using System.Net;
using FC.Codeflix.Catalog.Api.ApiModels.Response;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EndToEndTests.Api.Genre.CreateGenre;

[Collection(nameof(CreateGenreApiTestFixture))]
public class CreateGenreApiTest : IDisposable
{
    private readonly CreateGenreApiTestFixture _fixture;

    public CreateGenreApiTest(CreateGenreApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(CreateGenre))]
    [Trait("EndToEnd/API ", "Genre/Create Genre - Endpoints")]
    public  async Task CreateGenre()
    {
        var input = new CreateGenreInput(_fixture.GetValidCategoryName(), _fixture.GetRandomBoolean());
        //act
        var (response, output) = await _fixture.ApiClient
            .Post<ApiResponse<GenreModelOutput>>("/genres", input);
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status201Created);
        output.Should().NotBeNull();
        output!.Should().NotBeNull();
        output!.Data.Id.Should().NotBeEmpty();
        output!.Data.Name.Should().Be(input.Name);
        output!.Data.IsActive.Should().Be(input.IsActive);

        var genreFromDb = await _fixture.Persistence.GetById(output.Data.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(output.Data.Name);
        genreFromDb.IsActive.Should().Be(output.Data.IsActive);
    }
    
    [Fact(DisplayName = nameof(CreateGenre))]
    [Trait("EndToEnd/API ", "Genre/Create Genre - Endpoints")]
    public  async Task CreateGenreWithRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(10);
        _fixture.CategoryPersistence.BulkInsert(exampleCategories);
        
        var relatedCategories = exampleCategories.Skip(3).Take(3).Select(x => x.Id).ToList();
        
        var input = new CreateGenreInput(_fixture.GetValidCategoryName(), _fixture.GetRandomBoolean(), relatedCategories);
        //act
        var (response, output) = await _fixture.ApiClient
            .Post<ApiResponse<GenreModelOutput>>("/genres", input);
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status201Created);
        output!.Should().NotBeNull();
        output!.Data.Id.Should().NotBeEmpty();
        output!.Data.Name.Should().Be(input.Name);
        output!.Data.IsActive.Should().Be(input.IsActive);
        output.Data.Categories.Select(x => x.Id).ToList().Should().BeEquivalentTo(relatedCategories);

        var genreFromDb = await _fixture.Persistence.GetById(output.Data.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Name.Should().Be(output.Data.Name);
        genreFromDb.IsActive.Should().Be(output.Data.IsActive);
        var relationsFromDb = _fixture.Persistence.GetGenresCategoriesRelationsByGenreId(output.Data.Id)
            .Result.Select(x => x.CategoryId).ToList();
        relationsFromDb.Should().NotBeNull();
        relationsFromDb.Should().HaveCount(relatedCategories.Count);
        relationsFromDb.Should().BeEquivalentTo(relatedCategories);
    }
    
    [Fact(DisplayName = nameof(CreateGenreErrorWithInvalidRelations))]
    [Trait("EndToEnd/API ", "Genre/Create Genre - Endpoints")]
    public  async Task CreateGenreErrorWithInvalidRelations()
    {
        var exampleCategories = _fixture.GetExampleCategoriesList(10);
        await _fixture.CategoryPersistence.BulkInsert(exampleCategories!);
        
        var relatedCategories = exampleCategories.Skip(3).Take(3).Select(x => x.Id).ToList();
        var invalidCategoryId = Guid.NewGuid();
        relatedCategories.Add(invalidCategoryId);
        var input = new CreateGenreInput(_fixture.GetValidCategoryName(), _fixture.GetRandomBoolean(), relatedCategories);
        //act
        var (response, output) = await _fixture.ApiClient
            .Post<ProblemDetails>("/genres", input);
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status422UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Type.Should().Be("RelatedAggregate");
        output.Detail.Should().Be($"Related category id (or ids) not found: {invalidCategoryId}");

    }
    
    public void Dispose() => _fixture.CleanPersistence();
}