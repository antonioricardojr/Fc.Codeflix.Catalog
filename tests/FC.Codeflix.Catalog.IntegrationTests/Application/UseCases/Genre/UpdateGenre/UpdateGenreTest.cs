using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using FC.Codeflix.Catalog.Application.UseCases.Genre.GetGenre;
using FC.Codeflix.Catalog.Application.UseCases.Genre.UpdateGenre;
using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Models;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTest
{
    private readonly UpdateGenreTestFixture _fixture;

    public UpdateGenreTest(UpdateGenreTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(DisplayName = nameof(UpdateGenre))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenre()
    {
        
        var genreExampleList = _fixture.GetExampleListGenres(10);
        var arrangeDbContext = _fixture.CreateDbContext();
        var targetGenre = genreExampleList[4];
        await arrangeDbContext.AddRangeAsync(genreExampleList);
        await arrangeDbContext.SaveChangesAsync();
        
        
        var actDbContext = _fixture.CreateDbContext(true);
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre
            .UpdateGenre(
                new CategoryRepository(actDbContext), 
                new GenreRepository(actDbContext), 
                new UnitOfWork(actDbContext));
        var input = new UpdateGenreInput(targetGenre!.Id, _fixture.GetValidGenreName(), _fixture.GetRandomBoolean());
        var output = await useCase.Handle(input, CancellationToken.None);
        
        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().NotBe(targetGenre.Name);
        output.CreatedAt.Should().Be(targetGenre.CreatedAt);
        output.IsActive.Should().Be(input.IsActive!.Value);

        var assertDbContext = _fixture.CreateDbContext(true);
        var genreFromDb = assertDbContext.Genres.Find(targetGenre.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb.Id.Should().Be(output.Id);
        genreFromDb.Name.Should().Be(output.Name);
        genreFromDb.CreatedAt.Should().Be(output.CreatedAt);
        genreFromDb.IsActive.Should().Be(output.IsActive);
    }
    
    [Fact(DisplayName = nameof(UpdateGenreWithCategoriesRelations))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithCategoriesRelations()
    {
        List<Domain.Entity.Category> exampleCategories = _fixture.GetExampleCategoriesList(10)!;
        List<Domain.Entity.Genre> exampleGenres = _fixture.GetExampleListGenres(10)!;
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        Domain.Entity.Genre targetGenre = exampleGenres[5];
        List<Domain.Entity.Category> relatedCategories = exampleCategories.GetRange(0, 5);
        List<Domain.Entity.Category> newRelatedCategories = exampleCategories.GetRange(5, 3);
        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));
        List<GenresCategories> relations = targetGenre.Categories
            .Select(categoryId => new GenresCategories(categoryId, targetGenre.Id))
            .ToList();
        await arrangeDbContext.AddRangeAsync(exampleGenres);
        await arrangeDbContext.AddRangeAsync(exampleCategories);
        await arrangeDbContext.AddRangeAsync(relations);
        await arrangeDbContext.SaveChangesAsync();
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext(true);
        var unitOfWork = new UnitOfWork(actDbContext);
        var useCase = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            new CategoryRepository(actDbContext),
            new GenreRepository(actDbContext),
            unitOfWork
        );
        UpdateGenreInput input = new UpdateGenreInput(
            targetGenre.Id,
            _fixture.GetValidGenreName(),
            !targetGenre.IsActive,
            newRelatedCategories.Select(category => category.Id).ToList()
        );

        var output = await useCase.Handle(
            input,
            CancellationToken.None
        );

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);
        output.Categories.Should().HaveCount(newRelatedCategories.Count);
        List<Guid> relatedCategoryIdsFromOutput = 
            output.Categories.Select(relatedCategory => relatedCategory.Id).ToList();
        relatedCategoryIdsFromOutput.Should().BeEquivalentTo(input.CategoryIds);
        CodeflixCatalogDbContext assertDbContext = _fixture.CreateDbContext(true);
        Domain.Entity.Genre? genreFromDb =
            await assertDbContext.Genres.FindAsync(targetGenre.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);
        List<Guid> relatedcategoryIdsFromDb = await assertDbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == input.Id)
            .Select(relation => relation.CategoryId)
            .ToListAsync();
        relatedcategoryIdsFromDb.Should().BeEquivalentTo(input.CategoryIds);
    }
    
    [Fact(DisplayName = nameof(UpdateGenreWithoutNewCategoriesRelations))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithoutNewCategoriesRelations()
    {
        List<Domain.Entity.Category> exampleCategories = _fixture.GetExampleCategoriesList(10);
        List<Domain.Entity.Genre> exampleGenres = _fixture.GetExampleListGenres(10);
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        Domain.Entity.Genre targetGenre = exampleGenres[5];
        List<Domain.Entity.Category> relatedCategories = exampleCategories.GetRange(0, 5);
        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));
        List<GenresCategories> relations = targetGenre.Categories
            .Select(categoryId => new GenresCategories(categoryId, targetGenre.Id))
            .ToList();
        await arrangeDbContext.AddRangeAsync(exampleGenres);
        await arrangeDbContext.AddRangeAsync(exampleCategories);
        await arrangeDbContext.AddRangeAsync(relations);
        await arrangeDbContext.SaveChangesAsync();
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext(true);
        var unitOfWork = new UnitOfWork(actDbContext);
        var updateGenre = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            new CategoryRepository(actDbContext),
            new GenreRepository(actDbContext),
            unitOfWork);
        UpdateGenreInput input = new UpdateGenreInput(
            targetGenre.Id,
            _fixture.GetValidGenreName(),
            !targetGenre.IsActive
        );

        var output = await updateGenre.Handle(
            input,
            CancellationToken.None
        );

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);
        output.Categories.Should().HaveCount(relatedCategories.Count);
        List<Guid> expectedRelatedCategoryIds = relatedCategories
            .Select(category => category.Id).ToList();
        List<Guid> relatedCategoryIdsFromOutput =
            output.Categories.Select(relatedCategory => relatedCategory.Id).ToList();
        relatedCategoryIdsFromOutput.Should().BeEquivalentTo(expectedRelatedCategoryIds);
        CodeflixCatalogDbContext assertDbContext = _fixture.CreateDbContext(true);
        Domain.Entity.Genre? genreFromDb =
            await assertDbContext.Genres.FindAsync(targetGenre.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);
        List<Guid> relatedcategoryIdsFromDb = await assertDbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == input.Id)
            .Select(relation => relation.CategoryId)
            .ToListAsync();
        relatedcategoryIdsFromDb.Should().BeEquivalentTo(expectedRelatedCategoryIds);
    }
    
    [Fact(DisplayName = nameof(UpdateGenreWithEmptyCategoryIdsCleanRelations))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreWithEmptyCategoryIdsCleanRelations()
    {
        List<Domain.Entity.Category> exampleCategories = _fixture.GetExampleCategoriesList(10);
        List<Domain.Entity.Genre> exampleGenres = _fixture.GetExampleListGenres(10);
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        Domain.Entity.Genre targetGenre = exampleGenres[5];
        List<Domain.Entity.Category> relatedCategories = exampleCategories.GetRange(0, 5);
        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));
        List<GenresCategories> relations = targetGenre.Categories
            .Select(categoryId => new GenresCategories(categoryId, targetGenre.Id))
            .ToList();
        await arrangeDbContext.AddRangeAsync(exampleGenres);
        await arrangeDbContext.AddRangeAsync(exampleCategories);
        await arrangeDbContext.AddRangeAsync(relations);
        await arrangeDbContext.SaveChangesAsync();
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext(true);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var unitOfWork = new UnitOfWork(actDbContext);
        var updateGenre = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            new CategoryRepository(actDbContext),
            new GenreRepository(actDbContext),
            unitOfWork
        );
        UpdateGenreInput input = new UpdateGenreInput(
            targetGenre.Id,
            _fixture.GetValidGenreName(),
            !targetGenre.IsActive,
            new List<Guid>()
        );

        GenreModelOutput output = await updateGenre.Handle(
            input,
            CancellationToken.None
        );

        output.Should().NotBeNull();
        output.Id.Should().Be(targetGenre.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be((bool)input.IsActive!);
        output.Categories.Should().HaveCount(0);
        List<Guid> relatedCategoryIdsFromOutput =
            output.Categories.Select(relatedCategory => relatedCategory.Id).ToList();
        relatedCategoryIdsFromOutput.Should().BeEquivalentTo(new List<Guid>());
        CodeflixCatalogDbContext assertDbContext = _fixture.CreateDbContext(true);
        Domain.Entity.Genre? genreFromDb =
            await assertDbContext.Genres.FindAsync(targetGenre.Id);
        genreFromDb.Should().NotBeNull();
        genreFromDb!.Id.Should().Be(targetGenre.Id);
        genreFromDb.Name.Should().Be(input.Name);
        genreFromDb.IsActive.Should().Be((bool)input.IsActive!);
        List<Guid> relatedcategoryIdsFromDb = await assertDbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == input.Id)
            .Select(relation => relation.CategoryId)
            .ToListAsync();
        relatedcategoryIdsFromDb.Should().BeEquivalentTo(new List<Guid>());
    }
    
    [Fact(DisplayName = nameof(UpdateGenreThrowsWhenCategoryDoesntExists))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreThrowsWhenCategoryDoesntExists()
    {
        List<Domain.Entity.Category> exampleCategories = _fixture.GetExampleCategoriesList(10);
        List<Domain.Entity.Genre> exampleGenres = _fixture.GetExampleListGenres(10);
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        Domain.Entity.Genre targetGenre = exampleGenres[5];
        List<Domain.Entity.Category> relatedCategories = exampleCategories.GetRange(0, 5);
        List<Domain.Entity.Category> newRelatedCategories = exampleCategories.GetRange(5, 3);
        relatedCategories.ForEach(category => targetGenre.AddCategory(category.Id));
        List<GenresCategories> relations = targetGenre.Categories
            .Select(categoryId => new GenresCategories(categoryId, targetGenre.Id))
            .ToList();
        await arrangeDbContext.AddRangeAsync(exampleGenres);
        await arrangeDbContext.AddRangeAsync(exampleCategories);
        await arrangeDbContext.AddRangeAsync(relations);
        await arrangeDbContext.SaveChangesAsync();
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext(true);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var unitOfWork = new UnitOfWork(actDbContext);
        var updateGenre = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            new CategoryRepository(actDbContext),
            new GenreRepository(actDbContext),
            unitOfWork
        );
        List<Guid> categoryIdsToRelate = newRelatedCategories
            .Select(category => category.Id).ToList();
        Guid invalidCategoryId = Guid.NewGuid();
        categoryIdsToRelate.Add(invalidCategoryId);
        UpdateGenreInput input = new UpdateGenreInput(
            targetGenre.Id,
            _fixture.GetValidGenreName(),
            !targetGenre.IsActive,
            categoryIdsToRelate
        );

        Func<Task<GenreModelOutput>> action = 
            async () => await updateGenre.Handle(
                input,
                CancellationToken.None
            );

        await action.Should().ThrowAsync<RelatedAggregateException>()
            .WithMessage($"Related category id (or ids) not found: {invalidCategoryId}");
    }

    [Fact(DisplayName = nameof(UpdateGenreThrowsWhenNotFound))]
    [Trait("Integration/Application", "UpdateGenre - Use Cases")]
    public async Task UpdateGenreThrowsWhenNotFound()
    {
        List<Domain.Entity.Genre> exampleGenres = _fixture.GetExampleListGenres(10)!;
        CodeflixCatalogDbContext arrangeDbContext = _fixture.CreateDbContext();
        await arrangeDbContext.AddRangeAsync(exampleGenres);
        await arrangeDbContext.SaveChangesAsync();
        CodeflixCatalogDbContext actDbContext = _fixture.CreateDbContext(true);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var unitOfWork = new UnitOfWork(actDbContext);
        var updateGenre = new Catalog.Application.UseCases.Genre.UpdateGenre.UpdateGenre(
            new CategoryRepository(actDbContext),
            new GenreRepository(actDbContext),
            unitOfWork
        );
        Guid randomGuid = Guid.NewGuid();
        UpdateGenreInput input = new UpdateGenreInput(
            randomGuid,
            _fixture.GetValidGenreName(),
            true
        );

        Func<Task<GenreModelOutput>> action = 
            async () => await updateGenre.Handle(
                input,
                CancellationToken.None
            );

        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Genre '{randomGuid}' not found.");
    }
}