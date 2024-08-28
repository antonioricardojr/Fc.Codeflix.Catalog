using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.Interfaces;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;
using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest
{
    private readonly CreateGenreTestFixture _fixture;

    public CreateGenreTest(CreateGenreTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(CreateGenre))]
    [Trait("Integration/Application", "CreateGenre - Use Cases")]
    public async Task CreateGenre()
    {
        CreateGenreInput input = _fixture.GetExampleInput();
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext();
        GenreRepository genreRepository = new GenreRepository(actDbContext);
        IUnitOfWork unitOfWork = new UnitOfWork(actDbContext);
        CategoryRepository categoryRepository = new CategoryRepository(actDbContext);
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(genreRepository, unitOfWork, categoryRepository);

        GenreModelOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBe(default(DateTime));
        output.Categories.Should().HaveCount(0);
        CodeflixCatalogDbContext assertDbContext = _fixture.CreateDbContext(true);
        var genreFromDb = await assertDbContext.Genres.FindAsync(output.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Id.Should().Be(output.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be(input.IsActive);
        genreFromDb.CreatedAt.Should().Be(output.CreatedAt);
        genreFromDb.Categories.Should().HaveCount(0);

    }
    
    [Fact(DisplayName = nameof(CreateGenreWithCategoriesRelations))]
    [Trait("Integration/Application", "CreateGenre - Use Cases")]
    public async Task CreateGenreWithCategoriesRelations()
    {
        List<Domain.Entity.Category> categories = _fixture.GetExampleCategoriesList(5);
        CodeflixCatalogDbContext dbContext = _fixture.CreateDbContext();
        
        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();
        
        
        CreateGenreInput input = _fixture.GetExampleInput();
        input.CategoryIds = categories.Select(c => c.Id).ToList();
        GenreRepository genreRepository = new GenreRepository(dbContext);
        IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
        CategoryRepository categoryRepository = new CategoryRepository(dbContext);
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(genreRepository, unitOfWork, categoryRepository);

        GenreModelOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBe(default(DateTime));
        output.Categories.Should().HaveCount(input.CategoryIds.Count);
        output.Categories.Select(c => c.Id).ToList().Should().BeEquivalentTo(input.CategoryIds);
        CodeflixCatalogDbContext assertDbContext = _fixture.CreateDbContext(true);
        var genreFromDb = await assertDbContext.Genres.FindAsync(output.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Id.Should().Be(output.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be(input.IsActive);
        genreFromDb.CreatedAt.Should().Be(output.CreatedAt);
        genreFromDb.Categories.Should().HaveCount(0);
        List<GenresCategories> relations = await assertDbContext.GenresCategories.AsNoTracking()
            .Where(x => x.GenreId == output.Id).ToListAsync();
        relations.Should().HaveCount(input.CategoryIds.Count);
        List<Guid> categoryIdsRelatedFromDb = relations.Select(relation => relation.CategoryId).ToList();
        categoryIdsRelatedFromDb.Should().BeEquivalentTo(input.CategoryIds);
        
    }
    
    
    [Fact(DisplayName = nameof(CreateGenreThrowsWhenCategoryDoesNotExist))]
    [Trait("Integration/Application", "CreateGenre - Use Cases")]
    public async Task CreateGenreThrowsWhenCategoryDoesNotExist()
    {
        List<Domain.Entity.Category> categories = _fixture.GetExampleCategoriesList(5);
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.Categories.AddRangeAsync(categories);
        await arrangeDbContext.SaveChangesAsync();
        
        
        CreateGenreInput input = _fixture.GetExampleInput();
        input.CategoryIds = categories.Select(c => c.Id).ToList();
        var randomGuid = Guid.NewGuid();
        input.CategoryIds.Add(randomGuid);
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext();
        GenreRepository genreRepository = new GenreRepository(actDbContext);
        IUnitOfWork unitOfWork = new UnitOfWork(actDbContext);
        CategoryRepository categoryRepository = new CategoryRepository(actDbContext);
        var useCase = new Catalog.Application.UseCases.Genre.CreateGenre.CreateGenre(genreRepository, unitOfWork, categoryRepository);

        var action = async () => await useCase.Handle(input, CancellationToken.None);

        action.Should().ThrowAsync<RelatedAggregateException>().WithMessage($"Related category id (or ids) not found: {randomGuid}");


    }
}