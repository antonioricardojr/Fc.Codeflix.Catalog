using System.Net;
using EndToEndTests.Api.Genre.GetGenre;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EndToEndTests.Api.Genre.DeleteGenre;

[Collection(nameof(DeleteGenreApiTestFixture))]
public class DeleteGenreApiTest : IDisposable
{
    private readonly DeleteGenreApiTestFixture _fixture;

    public DeleteGenreApiTest(DeleteGenreApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("EndToEnd/API", "Genre/Delete - Endpoints")]
    public async Task DeleteGenre()
    {
        //arrange
        var exampleGenresList = _fixture.GetExampleGenresList(10);
        await _fixture.Persistence.BulkInsert(exampleGenresList);
        var exampleGenre = exampleGenresList[5];
        
        //act
        var (response, output) = await _fixture.ApiClient.Delete<object>($"/genres/{exampleGenre.Id}");
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status204NoContent);
        output.Should().BeNull();
        var persistenceGenre = await _fixture.Persistence.GetById(exampleGenre.Id);
        persistenceGenre.Should().BeNull();
    }
    
    [Fact(DisplayName = nameof(ErrorWhenNotFound))]
    [Trait("EndToEnd/API", "Genre/Delete - Endpoints")]
    public async Task ErrorWhenNotFound()
    {
        // arrange
        var exampleGenresList = _fixture.GetExampleGenresList(20);
        await _fixture.Persistence.BulkInsert(exampleGenresList);
        var randomGuid = Guid.NewGuid();

        // act
        var (response, output) = await _fixture.ApiClient.Delete<ProblemDetails>($"/genres/{randomGuid}");

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status404NotFound);
        output.Should().NotBeNull();
        output!.Title.Should().Be("Not Found");
        output.Detail.Should().Be($"Genre '{randomGuid}' not found.");
        output.Status.Should().Be(StatusCodes.Status404NotFound);
        output.Type.Should().Be("NotFound");
    }
    
    [Fact(DisplayName = nameof(DeleteGenreWithRelations))]
    [Trait("EndToEnd/API", "Genre/Delete - Endpoints")]
    public async Task DeleteGenreWithRelations()
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
        var (response, output) = await _fixture.ApiClient.Delete<object>($"/genres/{targetGenre.Id}");
        

        // assert
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((HttpStatusCode) StatusCodes.Status204NoContent);
        output.Should().BeNull();
        var persistenceGenre = await _fixture.Persistence.GetById(targetGenre.Id);
        persistenceGenre.Should().BeNull();
        var relations = await _fixture.Persistence.GetGenresCategoriesRelationsByGenreId(targetGenre.Id);
        relations.Should().HaveCount(0);
    }


    public void Dispose() => _fixture.CleanPersistence();
}