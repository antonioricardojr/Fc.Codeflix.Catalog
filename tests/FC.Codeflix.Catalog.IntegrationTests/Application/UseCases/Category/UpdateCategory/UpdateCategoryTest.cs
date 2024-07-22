using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Category.Common;
using FC.Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using FC.Codeflix.Catalog.Domain.Exceptions;
using FC.Codeflix.Catalog.Infra.Data.EF;
using FC.Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FC.Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTest
{
    private readonly UpdateCategoryTestFixture _fixture;

    public UpdateCategoryTest(UpdateCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Theory(DisplayName = nameof(UpdateCategory))]
    [Trait("Application ", "UpdateCategory - UseCases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategory(
        Catalog.Domain.Entity.Category exampleCategory, 
        UpdateCategoryInput input)
    {
        var dbContext = _fixture.CreateDbContext();
        var trackingInfo =await dbContext.AddAsync(exampleCategory);
        dbContext.SaveChanges();
        trackingInfo.State = EntityState.Detached;
        
        var unitOfWork = new UnitOfWork(dbContext);
        var repository = new CategoryRepository(dbContext);
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory
            .UpdateCategory(repository, unitOfWork);

        CategoryModelOutput output = await useCase.Handle(input, CancellationToken.None);
        
        output.Should().NotBeNull();
        output.Id.Should().Be(exampleCategory.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be((bool) input.IsActive!);
    }
    
    [Theory(DisplayName = nameof(UpdateCategoryWithoutIsActive))]
    [Trait("Application ", "UpdateCategory - UseCases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategoryWithoutIsActive(
        Catalog.Domain.Entity.Category exampleCategory, 
        UpdateCategoryInput exampleInput)
    {
        var input = new UpdateCategoryInput(exampleInput.Id, exampleInput.Name, exampleInput.Description);
        var dbContext = _fixture.CreateDbContext();
        var trackingInfo =await dbContext.AddAsync(exampleCategory);
        dbContext.SaveChanges();
        trackingInfo.State = EntityState.Detached;
        
        var unitOfWork = new UnitOfWork(dbContext);
        var repository = new CategoryRepository(dbContext);
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory
            .UpdateCategory(repository, unitOfWork);

        CategoryModelOutput output = await useCase.Handle(input, CancellationToken.None);
        
        output.Should().NotBeNull();
        output.Id.Should().Be(exampleCategory.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be((bool) exampleCategory.IsActive);
    }
    
    [Theory(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("Application ", "UpdateCategory - UseCases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategoryOnlyName(
        Catalog.Domain.Entity.Category exampleCategory, 
        UpdateCategoryInput exampleInput)
    {
        var input = new UpdateCategoryInput(exampleInput.Id, exampleInput.Name);
        var dbContext = _fixture.CreateDbContext();
        var trackingInfo =await dbContext.AddAsync(exampleCategory);
        dbContext.SaveChanges();
        trackingInfo.State = EntityState.Detached;
        
        var unitOfWork = new UnitOfWork(dbContext);
        var repository = new CategoryRepository(dbContext);
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory
            .UpdateCategory(repository, unitOfWork);

        CategoryModelOutput output = await useCase.Handle(input, CancellationToken.None);
        
        output.Should().NotBeNull();
        output.Id.Should().Be(exampleCategory.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(exampleCategory.Description);
        output.IsActive.Should().Be((bool) exampleCategory.IsActive);
    }
    
    [Theory(DisplayName = nameof(ThrowWhenCantInstantiateCategory))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 6,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async void ThrowWhenCantInstantiateCategory(
        UpdateCategoryInput input,
        string expectedExceptionMessage
    )
    {
        var dbContext = _fixture.CreateDbContext();
        var exampleCategories = _fixture.GetExampleCategoriesList();
        await dbContext.AddRangeAsync(exampleCategories);
        dbContext.SaveChanges();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory.UpdateCategory(repository, unitOfWork);
        input.Id = exampleCategories[0].Id;

        var task = async () 
            => await useCase.Handle(input, CancellationToken.None);
        
        await task.Should().ThrowAsync<EntityValidationException>()
            .WithMessage(expectedExceptionMessage);
    }
}