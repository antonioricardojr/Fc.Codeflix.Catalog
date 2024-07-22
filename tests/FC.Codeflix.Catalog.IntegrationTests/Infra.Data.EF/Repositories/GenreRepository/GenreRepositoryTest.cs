using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.GenreRepository;

[Collection(nameof(GenreRepositoryTestFixture))]
public class GenreRepositoryTest
{
    private readonly GenreRepositoryTestFixture _fixture;

    public GenreRepositoryTest(GenreRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task Insert()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList();
        categoriesListExample.ForEach(category => exampleGenre!.AddCategory(category!.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample!);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(dbContext);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await genreRepository.Insert(exampleGenre!, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre!.Id);

        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(exampleGenre.Name);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

        var genreCategoriesRelations =
            await assertsDbContext.GenresCategories.Where(r => r.GenreId == exampleGenre.Id).ToListAsync();
        genreCategoriesRelations.Should().HaveCount(categoriesListExample.Count);
        genreCategoriesRelations.ForEach(relation =>
        {
            var expectedCategory = categoriesListExample.FirstOrDefault(x => x.Id == relation.CategoryId);
            expectedCategory.Should().NotBeNull();

        });
    }
    
    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task Get()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList();
        categoriesListExample.ForEach(category => exampleGenre!.AddCategory(category!.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample!);
        await dbContext.Genres.AddAsync(exampleGenre!);

        foreach (var categoryId in exampleGenre!.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);
            await dbContext.GenresCategories.AddAsync(relation);
        }
        
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(dbContext);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var dbGenre = await genreRepository.Get(exampleGenre!.Id, CancellationToken.None);
        
        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(exampleGenre.Name);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var genreCategoriesRelations =
            await assertsDbContext.GenresCategories.Where(r => r.GenreId == exampleGenre.Id).ToListAsync();
        genreCategoriesRelations.Should().HaveCount(categoriesListExample.Count);
        genreCategoriesRelations.ForEach(relation =>
        {
            var expectedCategory = categoriesListExample.FirstOrDefault(x => x.Id == relation.CategoryId);
            expectedCategory.Should().NotBeNull();

        });
    }
    
    [Fact(DisplayName = nameof(ThrowNotFound))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task ThrowNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList();
        categoriesListExample.ForEach(category => exampleGenre!.AddCategory(category!.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample!);
        await dbContext.Genres.AddAsync(exampleGenre!);

        foreach (var categoryId in exampleGenre!.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);
            await dbContext.GenresCategories.AddAsync(relation);
        }
        
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(dbContext);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var id = Guid.NewGuid();
        var action = async () => await genreRepository.Get(id, CancellationToken.None);
        
        
        action.Should().NotBeNull();
        action.Should().ThrowAsync<NotFoundException>().WithMessage($"Genre '{id}' not found.");
    }
    
    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task Delete()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList();
        categoriesListExample.ForEach(category => exampleGenre!.AddCategory(category!.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample!);
        await dbContext.Genres.AddAsync(exampleGenre!);

        foreach (var categoryId in exampleGenre!.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);
            await dbContext.GenresCategories.AddAsync(relation);
        }
        await dbContext.SaveChangesAsync();
        var respositoryDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(respositoryDbContext);
        await genreRepository.Delete(exampleGenre, CancellationToken.None);
        await respositoryDbContext.SaveChangesAsync();
        
        var assertDbContext = _fixture.CreateDbContext(true);
        var dbGenre = await assertDbContext.Genres.FindAsync(exampleGenre!.Id);
        
        dbGenre.Should().BeNull();
        var categoriesIdsList = await 
            assertDbContext.GenresCategories.AsNoTracking()
                .Where(x => x.GenreId == exampleGenre.Id)
                .Select(x => x.CategoryId).ToListAsync();
        categoriesIdsList.Should().HaveCount(0);
        
    }
    
    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task Update()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList();
        categoriesListExample.ForEach(category => exampleGenre!.AddCategory(category!.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample!);
        await dbContext.Genres.AddAsync(exampleGenre!);

        foreach (var categoryId in exampleGenre!.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);
            await dbContext.GenresCategories.AddAsync(relation);
        }

        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var newName = _fixture.GetValidGenreName();
        exampleGenre.Update(newName);
        if (exampleGenre.IsActive)
        {
            exampleGenre.Activate();
        }
        else
        {
            exampleGenre.Deactivate();
        }
        
        await genreRepository.Update(exampleGenre, CancellationToken.None);
        await actDbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre.Id);
        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(newName);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

        var genreCategoriesRelations =
            await assertsDbContext.GenresCategories.Where(r => r.GenreId == exampleGenre.Id).ToListAsync();
        genreCategoriesRelations.Should().HaveCount(categoriesListExample.Count);
        genreCategoriesRelations.ForEach(relation =>
        {
            var expectedCategory = categoriesListExample.FirstOrDefault(x => x.Id == relation.CategoryId);
            expectedCategory.Should().NotBeNull();
        });
    }
    
    [Fact(DisplayName = nameof(UpdateRemovingCategories))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task UpdateRemovingCategories()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenre = _fixture.GetExampleGenre();
        var categoriesListExample = _fixture.GetExampleCategoriesList();
        var updateCategoriesListExample = _fixture.GetExampleCategoriesList(3);
        categoriesListExample.ForEach(category => exampleGenre!.AddCategory(category!.Id));

        await dbContext.Categories.AddRangeAsync(categoriesListExample!);
        await dbContext.Categories.AddRangeAsync(updateCategoriesListExample!);
        await dbContext.Genres.AddAsync(exampleGenre!);

        foreach (var categoryId in exampleGenre!.Categories)
        {
            var relation = new GenresCategories(categoryId, exampleGenre.Id);
            await dbContext.GenresCategories.AddAsync(relation);
        }

        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var newName = _fixture.GetValidGenreName();
        exampleGenre.Update(newName);
        if (exampleGenre.IsActive)
        {
            exampleGenre.Activate();
        }
        else
        {
            exampleGenre.Deactivate();
        }
        exampleGenre.RemoveAllCategories();
        updateCategoriesListExample.ForEach(x => exampleGenre.AddCategory(x.Id));
        await genreRepository.Update(exampleGenre, CancellationToken.None);
        await actDbContext.SaveChangesAsync(CancellationToken.None);

        var assertsDbContext = _fixture.CreateDbContext(true);
        var dbGenre = await assertsDbContext.Genres.FindAsync(exampleGenre.Id);
        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(newName);
        dbGenre.IsActive.Should().Be(exampleGenre.IsActive);
        dbGenre.CreatedAt.Should().Be(exampleGenre.CreatedAt);

        var genreCategoriesRelations =
            await assertsDbContext.GenresCategories.Where(r => r.GenreId == exampleGenre.Id).ToListAsync();
        genreCategoriesRelations.Should().HaveCount(updateCategoriesListExample.Count);
        genreCategoriesRelations.ForEach(relation =>
        {
            var expectedCategory = updateCategoriesListExample.FirstOrDefault(x => x.Id == relation.CategoryId);
            expectedCategory.Should().NotBeNull();
        });
    }
    
    [Fact(DisplayName = nameof(ListReturnItemsAndCount))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task ListReturnItemsAndCount()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenresList = _fixture.GetExampleListGenres();
        await dbContext.Genres.AddRangeAsync(exampleGenresList!);
        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);
        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(exampleGenresList.Count);
        searchResult.Items.Should().HaveCount(exampleGenresList.Count);

        foreach (var resultItem in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == resultItem.Id);
            exampleGenre!.Id.Should().Be(resultItem.Id);
            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(resultItem.Name);
            exampleGenre.IsActive.Should().Be(resultItem.IsActive);
            exampleGenre.CreatedAt.Should().Be(resultItem.CreatedAt);
        }
    }
    
    [Fact(DisplayName = nameof(SearchReturnsRelations))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task SearchReturnsRelations()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenresList = _fixture.GetExampleListGenres();
        await dbContext.Genres.AddRangeAsync(exampleGenresList!);
        var random = new Random();
        exampleGenresList.ForEach(exampleGenre =>
        {
            var categoriesListToRelate = _fixture.GetExampleCategoriesList(random.Next(0, 4));
            if (categoriesListToRelate.Count > 0)
            {
                categoriesListToRelate.ForEach(category => exampleGenre!.AddCategory(category!.Id));
                dbContext.Categories.AddRangeAsync(categoriesListToRelate!, CancellationToken.None);
                var relationsToAdd =
                    categoriesListToRelate.Select(category => new GenresCategories(category!.Id, exampleGenre!.Id));
                dbContext.GenresCategories.AddRangeAsync(relationsToAdd, CancellationToken.None);
            }
        });
        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);
        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(exampleGenresList.Count);
        searchResult.Items.Should().HaveCount(exampleGenresList.Count);

        foreach (var resultItem in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == resultItem.Id);
            exampleGenre!.Id.Should().Be(resultItem.Id);
            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(resultItem.Name);
            exampleGenre.IsActive.Should().Be(resultItem.IsActive);
            exampleGenre.CreatedAt.Should().Be(resultItem.CreatedAt);
            resultItem.Categories.Should().BeEquivalentTo(exampleGenre.Categories);
        }
    }
    
    [Fact(DisplayName = nameof(SearchReturnsEmptyWhenPersistenceIsEmpty))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    public async Task SearchReturnsEmptyWhenPersistenceIsEmpty()
    {
        var dbContext = _fixture.CreateDbContext();
        var random = new Random();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);
        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(0);
        searchResult.Items.Should().HaveCount(0);
    }
    
    [Theory(DisplayName = nameof(SearchReturnsRelations))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchReturnsPaginated(int numGenresToGenerate, int page, int perPage, int expectedNumItems)
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenresList = _fixture.GetExampleListGenres(numGenresToGenerate);
        await dbContext.Genres.AddRangeAsync(exampleGenresList!);
        var random = new Random();
        exampleGenresList.ForEach(exampleGenre =>
        {
            var categoriesListToRelate = _fixture.GetExampleCategoriesList(random.Next(0, 4));
            if (categoriesListToRelate.Count > 0)
            {
                categoriesListToRelate.ForEach(category => exampleGenre!.AddCategory(category!.Id));
                dbContext.Categories.AddRangeAsync(categoriesListToRelate!, CancellationToken.None);
                var relationsToAdd =
                    categoriesListToRelate.Select(category => new GenresCategories(category!.Id, exampleGenre!.Id));
                dbContext.GenresCategories.AddRangeAsync(relationsToAdd, CancellationToken.None);
            }
        });
        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.Asc);
        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(exampleGenresList.Count);
        searchResult.Items.Should().HaveCount(expectedNumItems);

        foreach (var resultItem in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == resultItem.Id);
            exampleGenre!.Id.Should().Be(resultItem.Id);
            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(resultItem.Name);
            exampleGenre.IsActive.Should().Be(resultItem.IsActive);
            exampleGenre.CreatedAt.Should().Be(resultItem.CreatedAt);
            resultItem.Categories.Should().BeEquivalentTo(exampleGenre.Categories);
        }
    }
    
    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
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
        int expectedQuantityTotalItems
    )
    {
        var dbContext = _fixture.CreateDbContext();
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
        await dbContext.Genres.AddRangeAsync(exampleGenresList!);
        var random = new Random();
        exampleGenresList.ForEach(exampleGenre =>
        {
            var categoriesListToRelate = _fixture.GetExampleCategoriesList(random.Next(0, 4));
            if (categoriesListToRelate.Count > 0)
            {
                categoriesListToRelate.ForEach(category => exampleGenre!.AddCategory(category!.Id));
                dbContext.Categories.AddRangeAsync(categoriesListToRelate!, CancellationToken.None);
                var relationsToAdd =
                    categoriesListToRelate.Select(category => new GenresCategories(category!.Id, exampleGenre!.Id));
                dbContext.GenresCategories.AddRangeAsync(relationsToAdd, CancellationToken.None);
            }
        });
        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.Asc);
        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(expectedQuantityTotalItems);
        searchResult.Items.Should().HaveCount(expectedQuantityItemsReturned);

        foreach (var resultItem in searchResult.Items)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == resultItem.Id);
            exampleGenre!.Id.Should().Be(resultItem.Id);
            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(resultItem.Name);
            exampleGenre.IsActive.Should().Be(resultItem.IsActive);
            exampleGenre.CreatedAt.Should().Be(resultItem.CreatedAt);
            resultItem.Categories.Should().BeEquivalentTo(exampleGenre.Categories);
        }
    }
    
    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Infra.Data", "Genre - Repositories")]
    [InlineData("name","asc")]
    [InlineData("name","desc")]
    [InlineData("id","asc")]
    [InlineData("id","desc")]
    [InlineData("createdAt","asc")]
    [InlineData("createdAt","desc")]
    public async Task SearchOrdered(        
        string orderBy,
        string order
    )
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleGenresList = _fixture.GetExampleListGenres();
        await dbContext.Genres.AddRangeAsync(exampleGenresList!);
        var random = new Random();
        exampleGenresList.ForEach(exampleGenre =>
        {
            var categoriesListToRelate = _fixture.GetExampleCategoriesList(random.Next(0, 4));
            if (categoriesListToRelate.Count > 0)
            {
                categoriesListToRelate.ForEach(category => exampleGenre!.AddCategory(category!.Id));
                dbContext.Categories.AddRangeAsync(categoriesListToRelate!, CancellationToken.None);
                var relationsToAdd =
                    categoriesListToRelate.Select(category => new GenresCategories(category!.Id, exampleGenre!.Id));
                dbContext.GenresCategories.AddRangeAsync(relationsToAdd, CancellationToken.None);
            }
        });
        await dbContext.SaveChangesAsync();

        var actDbContext = _fixture.CreateDbContext(true);
        var genreRepository = new Catalog.Infra.Data.EF.Repositories.GenreRepository(actDbContext);
        var searchOrder = order.ToLower() == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var searchInput = new SearchInput(1, 10, "", orderBy, searchOrder);
        var searchResult = await genreRepository.Search(searchInput, CancellationToken.None);

        var expectedOrderedList = _fixture.CloneGenresListOrdered(exampleGenresList, orderBy, searchOrder);
        
        searchResult.Should().NotBeNull();
        searchResult.CurrentPage.Should().Be(searchInput.Page);
        searchResult.PerPage.Should().Be(searchInput.PerPage);
        searchResult.Total.Should().Be(expectedOrderedList.Count);
        searchResult.Items.Should().HaveCount(expectedOrderedList.Count);

        foreach (var orderedItem in expectedOrderedList)
        {
            var exampleGenre = exampleGenresList.Find(x => x.Id == orderedItem.Id);
            exampleGenre!.Id.Should().Be(orderedItem.Id);
            exampleGenre.Should().NotBeNull();
            exampleGenre!.Name.Should().Be(orderedItem.Name);
            exampleGenre.IsActive.Should().Be(orderedItem.IsActive);
            exampleGenre.CreatedAt.Should().Be(orderedItem.CreatedAt);
            orderedItem.Categories.Should().BeEquivalentTo(exampleGenre.Categories);
        }
    }
}