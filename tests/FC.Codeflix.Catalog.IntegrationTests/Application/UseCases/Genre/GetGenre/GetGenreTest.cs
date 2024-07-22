using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Genre.GetGenre;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.GetGenre;

[Collection(nameof(GetGenreTestFixture))]
public class GetGenreTest
{
    private readonly GetGenreTestFixture _fixture;

    public GetGenreTest(GetGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("Integration/Application", "GetGenre - Use Cases")]
    public async Task GetGenre()
    {
        var genreExampleList = _fixture.GetExampleListGenres(10);
        var expectedGenre = genreExampleList[4];
        var dbArrangeContext = _fixture.CreateDbContext();
        await dbArrangeContext.Genres.AddRangeAsync(genreExampleList!);
        await dbArrangeContext.SaveChangesAsync();
        var genreRepository = new GenreRepository(_fixture.CreateDbContext(true));
        var categoryRepository = new CategoryRepository(dbArrangeContext);
        var useCase = new Catalog.Application.UseCases.Genre.GetGenre.GetGenre(genreRepository, categoryRepository);
        var input = new GetGenreInput(expectedGenre!.Id);


        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(expectedGenre.Id);
        output.Name.Should().Be(expectedGenre.Name);
        output.CreatedAt.Should().Be(expectedGenre.CreatedAt);
        output.IsActive.Should().Be(expectedGenre.IsActive);
    }
    
    [Fact(DisplayName = nameof(GetGenreThrowsWhenNotFound))]
    [Trait("Integration/Application", "GetGenre - Use Cases")]
    public async Task GetGenreThrowsWhenNotFound()
    {
        var genreExampleList = _fixture.GetExampleListGenres(10);
        var dbArrangeContext = _fixture.CreateDbContext();
        await dbArrangeContext.Genres.AddRangeAsync(genreExampleList!);
        await dbArrangeContext.SaveChangesAsync();
        var genreRepository = new GenreRepository(_fixture.CreateDbContext(true));
        var categoryRepository = new CategoryRepository(dbArrangeContext);
        var useCase = new Catalog.Application.UseCases.Genre.GetGenre.GetGenre(genreRepository, categoryRepository);
        var randomGuid = Guid.NewGuid();
        var input = new GetGenreInput(randomGuid);


        var action = async () => await useCase.Handle(input, CancellationToken.None);

        action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{randomGuid}' not found.");

    }
    
    [Fact(DisplayName = nameof(GetGenreWithCategoryRelations))]
    [Trait("Integration/Application", "GetGenre - Use Cases")]
    public async Task GetGenreWithCategoryRelations()
    {
        var genresExampleList = _fixture.GetExampleListGenres(10);
        var categoriesExampleList = _fixture.GetExampleCategoriesList(5);
        var expectedGenre = genresExampleList[5];
        categoriesExampleList.ForEach(
            category => expectedGenre.AddCategory(category.Id)
        );
        var dbArrangeContext = _fixture.CreateDbContext();
        await dbArrangeContext.Categories.AddRangeAsync(categoriesExampleList);
        await dbArrangeContext.Genres.AddRangeAsync(genresExampleList);
        await dbArrangeContext.GenresCategories.AddRangeAsync(
            expectedGenre.Categories.Select(
                categoryId => new GenresCategories(
                    categoryId, 
                    expectedGenre.Id
                )
            )
        );
        await dbArrangeContext.SaveChangesAsync();
        var dbActContext = _fixture.CreateDbContext(true);
        var categoryRepository = new CategoryRepository(dbArrangeContext);
        var genreRepository = new GenreRepository(dbActContext);
        var useCase = new Catalog.Application.UseCases.Genre.GetGenre.GetGenre(genreRepository, categoryRepository);
        var input = new GetGenreInput(expectedGenre.Id);


        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(expectedGenre.Id);
        output.Name.Should().Be(expectedGenre.Name);
        output.CreatedAt.Should().Be(expectedGenre.CreatedAt);
        output.IsActive.Should().Be(expectedGenre.IsActive);
        output.Categories.Should().HaveCount(expectedGenre.Categories.Count);
        output.Categories.ToList().ForEach(relationModel =>
        {
            relationModel.Name.Should().NotBeNull();
            expectedGenre.Categories.Should().Contain(relationModel.Id);
        });
        
    }
}