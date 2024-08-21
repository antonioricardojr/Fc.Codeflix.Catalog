using System.Net;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EndToEndTests.Api.Genre.GetGenre;

class GetGenreResponse
{
    public GenreModelOutput Data { get; set;  }
    public GetGenreResponse(GenreModelOutput data)
    {
        Data = data;
    }
}



[Collection(nameof(GetGenreApiTestFixture))]
public class GetGenreApiTest : IDisposable
{
    private readonly GetGenreApiTestFixture _fixture;

    public GetGenreApiTest(GetGenreApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("EndToEnd/API", "Genre/Get - Endpoints")]
    public async Task GetGenre()
    {
        // arrange
        var exampleGenresList = _fixture.GetExampleGenresList(20);
        await _fixture.Persistence.BulkInsert(exampleGenresList);
        var exampleGenre = exampleGenresList[10];
        
        //act
        var (response, output) = await _fixture.ApiClient.Get<GetGenreResponse>($"/genres/{exampleGenre.Id}");
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output!.Data.Id.Should().Be(exampleGenre.Id);
        output!.Data.Name.Should().Be(exampleGenre.Name);
        output!.Data.IsActive.Should().Be(exampleGenre.IsActive);
        output!.Data.CreatedAt.Should().Be(exampleGenre.CreatedAt);
    }
    
    [Fact(DisplayName = nameof(ErrorWhenNotFound))]
    [Trait("EndToEnd/API", "Genre/Get - Endpoints")]
    public async Task ErrorWhenNotFound()
    {
        // arrange
        var exampleGenresList = _fixture.GetExampleGenresList(20);
        await _fixture.Persistence.BulkInsert(exampleGenresList);
        var randomId = Guid.NewGuid();
        
        //act
        var (response, output) = await _fixture.ApiClient.Get<ProblemDetails>($"/genres/{randomId}");
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status404NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status404NotFound);
        output!.Title.Should().Be("Not Found");
        output!.Detail.Should().Be($"Genre '{randomId}' not found.");
        output.Type.Should().Be("NotFound");
    }
    
    [Fact(DisplayName = nameof(GetGenreWithRelations))]
    [Trait("EndToEnd/API", "Genre/Get - Endpoints")]
    public async Task GetGenreWithRelations()
    {
        // arrange
        var genres = _fixture.GetExampleGenresList(10);
        var categories = _fixture.GetExampleCategoriesList(10);
        var targetGenre = genres[5];
        Random random = new Random();
        genres.ForEach(genre =>
        {
            int relationsCount = random.Next(0, 3);
            for (int i = 0; i < relationsCount; i++)
            {
                var selectedCategoryIndex = random.Next(0, categories.Count - 1);
                var selectedCategory = categories[selectedCategoryIndex];
                if (!genre!.Categories.Contains(selectedCategory!.Id))
                {
                    genre!.AddCategory(selectedCategory!.Id);
                }
            }
        });
        List<GenresCategories> genresCategories = new List<GenresCategories>();
        genres.ForEach(genre =>
        {
            genre!.Categories.ToList().ForEach(c =>
            {
                genresCategories.Add(new GenresCategories(c, genre.Id));
            });
        });
        
        await _fixture.Persistence.BulkInsert(genres);
        await _fixture.CategoryPersistence.BulkInsert(categories);
        await _fixture.Persistence.BulkInsertGenresCategoriesRelationsList(genresCategories);
        
        //act
        var (response, output) = await _fixture.ApiClient.Get<GetGenreResponse>($"/genres/{targetGenre.Id}");
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status200OK);
        output.Should().NotBeNull();
        output!.Data.Should().NotBeNull();
        output!.Data.Id.Should().Be(targetGenre.Id);
        output!.Data.Name.Should().Be(targetGenre.Name);
        output!.Data.IsActive.Should().Be(targetGenre.IsActive);
        output!.Data.CreatedAt.Should().Be(targetGenre.CreatedAt);
        var relatedCategoryIds = output.Data.Categories.Select(relation => relation.Id).ToList();
        relatedCategoryIds.Should().BeEquivalentTo(targetGenre.Categories);
    }

    public void Dispose() => _fixture.CleanPersistence();
}