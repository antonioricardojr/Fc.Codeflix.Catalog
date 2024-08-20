using FC.Codeflix.Catalog.Application.UseCases.Genre.ListGenres;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
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

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

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

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

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

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

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
            item.Categories.ToList().ForEach(outputCategory =>
            {
                Domain.Entity.Category exampleCategort = categories.Find(x => x.Id == outputCategory.Id);
                exampleCategort.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategort!.Name);
            });
        });
    }
    
    [Theory(DisplayName = nameof(ListGenresPaginated))]
    [Trait("Integration/Application", "ListGenres - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListGenresPaginated(int numGenresToGenerate, int page, int perPage, int expectedNumItems)
    {
        var genres = _fixture.GetExampleListGenres(numGenresToGenerate);
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

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

        var input = new ListGenresInput(page, perPage);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        output.Items.Should().HaveCount(expectedNumItems);

        output.Items.ToList().ForEach(item =>
        {
            Domain.Entity.Genre genre = genres.Find(genre => genre.Id == item.Id);
            genre.Should().NotBeNull();
            item.Name.Should().Be(genre!.Name);
            item.IsActive.Should().Be(genre.IsActive);
            List<Guid> itemCategoryIds = item.Categories.Select(x => x.Id).ToList();
            itemCategoryIds.Should().BeEquivalentTo(item.Categories.Select(i => i.Id).ToList());
            item.Categories.ToList().ForEach(outputCategory =>
            {
                Domain.Entity.Category exampleCategort = categories.Find(x => x.Id == outputCategory.Id);
                exampleCategort.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategort!.Name);

            });
            
        });
        
    }
    
    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListGenres - Use Cases")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-fi", 1, 5, 4, 4)]
    [InlineData("Sci-fi", 1, 2, 2, 4)]
    [InlineData("Sci-fi", 2, 3, 1, 4)]
    [InlineData("Sci-fi Other", 1, 3, 0, 0)]
    [InlineData("Robots", 1, 5, 2, 2)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityItemsReturned,
        int expectedQuantityTotalItems)
    {
        var categories = _fixture.GetExampleCategoriesList(10);

        var exampleGenresList = _fixture.GetExampleListGenresByNames(new ()
        {                
            "Action",
            "Horror",
            "Horror - Robots",
            "Horror - Based on Real Facts",
            "Drama",
            "Sci-fi IA",
            "Sci-fi Space",
            "Sci-fi Robots",
            "Sci-fi Future"});
        Random random = new Random();
        exampleGenresList.ForEach(genre =>
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
        exampleGenresList.ForEach(genre =>
        {
            genre!.Categories.ToList().ForEach(c =>
            {
                genresCategories.Add(new GenresCategories(c, genre.Id));
            });
        });
        
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.Genres.AddRangeAsync(exampleGenresList!);
        await arrangeDbContext.Categories.AddRangeAsync(categories!);
        await arrangeDbContext.GenresCategories.AddRangeAsync(genresCategories);
        await arrangeDbContext.SaveChangesAsync();

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

        var input = new ListGenresInput(page, perPage, search);
        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Items.Should().HaveCount(expectedQuantityItemsReturned);

        output.Items.ToList().ForEach(item =>
        {
            item.Name.Should().Contain(search);
            Domain.Entity.Genre genre = exampleGenresList.Find(genre => genre.Id == item.Id);
            genre.Should().NotBeNull();
            item.Name.Should().Be(genre!.Name);
            item.IsActive.Should().Be(genre.IsActive);
            List<Guid> itemCategoryIds = item.Categories.Select(x => x.Id).ToList();
            itemCategoryIds.Should().BeEquivalentTo(item.Categories.Select(i => i.Id).ToList());
            item.Categories.ToList().ForEach(outputCategory =>
            {
                Domain.Entity.Category exampleCategort = categories.Find(x => x.Id == outputCategory.Id);
                exampleCategort.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategort!.Name);

            });
            
        });
        
    }
    
    [Theory(DisplayName = nameof(Ordered))]
    [Trait("Integration/Application", "ListGenres - Use Cases")]
    [InlineData("name","asc")]
    [InlineData("name","desc")]
    [InlineData("id","asc")]
    [InlineData("id","desc")]
    [InlineData("createdAt","asc")]
    [InlineData("createdAt","desc")]
    public async Task Ordered(string orderBy, string order)
    {
        var genres = _fixture.GetExampleListGenres();
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

        var useCase = new Catalog.Application.UseCases.Genre.ListGenres.ListGenres(new GenreRepository(arrangeDbContext), new CategoryRepository(arrangeDbContext));

        var orderEnum = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListGenresInput(1, 20, sort: order, dir: orderEnum);
        var output = await useCase.Handle(input, CancellationToken.None);

        var expectedOrderedList = _fixture.CloneGenresListOrdered(genres, orderBy, orderEnum);
        
        foreach (var orderedItem in expectedOrderedList)
        {
            var exampleGenre = genres.Find(x => x.Id == orderedItem.Id);
            exampleGenre!.Id.Should().Be(orderedItem.Id);
            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(orderedItem.Name);
            exampleGenre.IsActive.Should().Be(orderedItem.IsActive);
            exampleGenre.CreatedAt.Should().Be(orderedItem.CreatedAt);
            orderedItem.Categories.Should().BeEquivalentTo(exampleGenre.Categories);
        }
        
        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(genres.Count);
        output.Items.Should().HaveCount(genres.Count);
        output.Items.ToList().ForEach(item =>
        {
            Domain.Entity.Genre genre = genres.Find(genre => genre.Id == item.Id);
            genre.Should().NotBeNull();
            item.Name.Should().Be(genre!.Name);
            item.IsActive.Should().Be(genre.IsActive);
            List<Guid> itemCategoryIds = item.Categories.Select(x => x.Id).ToList();
            itemCategoryIds.Should().BeEquivalentTo(item.Categories.Select(i => i.Id).ToList());
            item.Categories.ToList().ForEach(outputCategory =>
            {
                Domain.Entity.Category exampleCategort = categories.Find(x => x.Id == outputCategory.Id);
                exampleCategort.Should().NotBeNull();
                outputCategory.Name.Should().Be(exampleCategort!.Name);
            });
        });
    }
}