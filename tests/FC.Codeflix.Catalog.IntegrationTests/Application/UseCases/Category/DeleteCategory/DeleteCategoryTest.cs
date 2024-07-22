using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Category.DeleteCategory;
using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest
{
    private readonly DeleteCategoryTestFixture _fixture;

    public DeleteCategoryTest(DeleteCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Integration/Application", "DeleteCategory - UseCases")]
    public async Task DeleteCategory()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var exampleList = _fixture.GetExampleCategoriesList(10);
        await dbContext.AddRangeAsync(exampleList);
        var tracking =await dbContext.AddAsync(exampleCategory);
        dbContext.SaveChanges();
        tracking.State = EntityState.Detached;
        
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new Catalog.Application.UseCases.Category.DeleteCategory.DeleteCategory(repository, unitOfWork);
        
        
        var input = new DeleteCategoryInput(exampleCategory.Id);
        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = _fixture.CreateDbContext(true);
        var dbCategoryDelete = await assertDbContext.Categories.FindAsync(exampleCategory.Id);
        dbCategoryDelete.Should().BeNull();
        var dbCategoriesCount = await assertDbContext.Categories.ToListAsync();
        dbCategoriesCount.Should().HaveCount(exampleList.Count);

    }
    
    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Integration/Application", "DeleteCategory - UseCases")]
    public async Task DeleteCategoryThrowsWhenNotFound()
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategory = _fixture.GetExampleCategory();
        var exampleList = _fixture.GetExampleCategoriesList(10);
        await dbContext.AddRangeAsync(exampleList);
        var tracking =await dbContext.AddAsync(exampleCategory);
        dbContext.SaveChanges();
        tracking.State = EntityState.Detached;
        
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new Catalog.Application.UseCases.Category.DeleteCategory.DeleteCategory(repository, unitOfWork);

        var id = Guid.NewGuid();
        var input = new DeleteCategoryInput(id);
        var task = async () => await useCase.Handle(input, CancellationToken.None);

        task.Should().ThrowAsync<NotFoundException>().WithMessage($"Category '{id}' not found.");

    }
}