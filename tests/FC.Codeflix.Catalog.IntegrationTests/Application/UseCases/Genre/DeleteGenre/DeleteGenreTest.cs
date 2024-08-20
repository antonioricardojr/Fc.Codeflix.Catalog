using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Genre.DeleteGenre;
using FC.Codeflix.Catalog.Application.UseCases.Genre.GetGenre;
using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.DeleteGenre;

[Collection(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTest
{
    private readonly DeleteGenreTestFixture _fixture;

    public DeleteGenreTest(DeleteGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("Integration/Application", "Delete Genre - Use Cases")]
    public async Task DeleteGenre()
    {
        var genreExampleList = _fixture.GetExampleListGenres(10);
        var targetGenre = genreExampleList[4];
        var dbArrangeContext = _fixture.CreateDbContext();
        await dbArrangeContext.Genres.AddRangeAsync(genreExampleList!);
        await dbArrangeContext.SaveChangesAsync();
        var actDbContext = _fixture.CreateDbContext(true);
        var useCase = new Catalog.Application.UseCases.Genre.DeleteGenre.DeleteGenre(new GenreRepository(actDbContext), new UnitOfWork(actDbContext));
        var input = new DeleteGenreInput(targetGenre!.Id);


        await useCase.Handle(input, CancellationToken.None);
        
        var assertDbContext = _fixture.CreateDbContext(true);

        var genreOutput = await assertDbContext.Genres.FindAsync(targetGenre.Id);
        genreOutput.Should().BeNull();
    }
    
    [Fact(DisplayName = nameof(DeleteGenreWithRelations))]
    [Trait("Integration/Application", "Delete Genre - Use Cases")]
    public async Task DeleteGenreWithRelations()
    {
        var genreExampleList = _fixture.GetExampleListGenres(10);
        var targetGenre = genreExampleList[4];
        var exampleCategories = _fixture.GetExampleCategoriesList(5);
        var dbArrangeContext = _fixture.CreateDbContext();
        await dbArrangeContext.Categories.AddRangeAsync(exampleCategories!);
        await dbArrangeContext.Genres.AddRangeAsync(genreExampleList!);
        await dbArrangeContext.GenresCategories.AddRangeAsync(
            exampleCategories.Select(category => new GenresCategories(category!.Id, targetGenre!.Id))
            );
        await dbArrangeContext.SaveChangesAsync();
        var actDbContext = _fixture.CreateDbContext(true);
        var useCase = new Catalog.Application.UseCases.Genre.DeleteGenre.DeleteGenre(new GenreRepository(actDbContext), new UnitOfWork(actDbContext));
        var input = new DeleteGenreInput(targetGenre!.Id);
        
        await useCase.Handle(input, CancellationToken.None);
        
        var assertDbContext = _fixture.CreateDbContext(true);
        var genreOutput = await assertDbContext.Genres.FindAsync(targetGenre.Id);
        genreOutput.Should().BeNull();

        var relations = await assertDbContext.GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == targetGenre.Id).ToListAsync();
        relations.Should().BeEmpty();
        
    }
    
    
    [Fact(DisplayName = nameof(DeleteGenreThrowsWhenNotFound))]
    [Trait("Integration/Application", "Delete Genre - Use Cases")]
    public async Task DeleteGenreThrowsWhenNotFound()
    {
        var genreExampleList = _fixture.GetExampleListGenres(10);
        var dbArrangeContext = _fixture.CreateDbContext();
        await dbArrangeContext.Genres.AddRangeAsync(genreExampleList!);
        await dbArrangeContext.SaveChangesAsync();
        var actDbContext = _fixture.CreateDbContext(true);
        var useCase = new Catalog.Application.UseCases.Genre.DeleteGenre.DeleteGenre(new GenreRepository(actDbContext), new UnitOfWork(actDbContext));
        var randomGuid = Guid.NewGuid();
        var input = new DeleteGenreInput(randomGuid);


        Func<Task> action = async () => await useCase.Handle(input, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{randomGuid}' not found.");
    }
}