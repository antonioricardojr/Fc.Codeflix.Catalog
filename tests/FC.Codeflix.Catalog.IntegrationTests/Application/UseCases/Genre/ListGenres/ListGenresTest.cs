using FC.Codeflix.Catalog.Application.UseCases.Genre.ListGenres;
using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.ListGenres;

[Collection(nameof(ListGenresTestFixture))]
public class ListGenresTest
{
    private readonly ListGenresTestFixture _fixture;

    public ListGenresTest(ListGenresTestFixture fixture)
    {
        _fixture = fixture;
    }


    [Fact(DisplayName = nameof(ListGenres))]
    [Trait("Integration/Application", "ListGenres - Use Cases")]
    public async Task ListGenres()
    {
        List<Domain.Entity.Genre> genres = _fixture.GetExampleListGenres(10);
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.Genres.AddRangeAsync(genres);
        await arrangeDbContext.SaveChangesAsync();

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext));

        var input = new ListGenresInput();
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Items.Should().NotBeEmpty();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        foreach (var genreId in output.Items.Select(genre => genre.Id))
        {
            genres.Select(g => g.Id).Should().Contain(genreId);
        }
        
    }
    
    [Fact(DisplayName = nameof(ListGenresReturnsEmptyWhenPersistenteIsEmpty))]
    [Trait("Integration/Application", "ListGenres - Use Cases")]
    public async Task ListGenresReturnsEmptyWhenPersistenteIsEmpty()
    {
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext));

        var input = new ListGenresInput();
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Items.Should().BeEmpty();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        
    }
    
    [Fact(DisplayName = nameof(ListGenresVerifyRelations))]
    [Trait("Integration/Application", "ListGenres - Use Cases")]
    public async Task ListGenresVerifyRelations()
    {
        var genres = _fixture.GetExampleListGenres(10);
        var categories = _fixture.GetExampleCategoriesList(10);

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
        
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.Genres.AddRangeAsync(genres!);
        await arrangeDbContext.Categories.AddRangeAsync(categories!);
        await arrangeDbContext.GenresCategories.AddRangeAsync(genresCategories);
        await arrangeDbContext.SaveChangesAsync();

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext));

        var input = new ListGenresInput();
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Items.Should().NotBeEmpty();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);

        output.Items.ToList().ForEach(item =>
        {
            Domain.Entity.Genre genre = genres.Find(genre => genre.Id == item.Id);
            genre.Should().NotBeNull();
            item.Name.Should().Be(genre!.Name);
            item.IsActive.Should().Be(genre.IsActive);
            List<Guid> itemCategoryIds = item.Categories.Select(x => x.Id).ToList();
            itemCategoryIds.Should().BeEquivalentTo(item.Categories.Select(i => i.Id).ToList());
        });
        
    }
}