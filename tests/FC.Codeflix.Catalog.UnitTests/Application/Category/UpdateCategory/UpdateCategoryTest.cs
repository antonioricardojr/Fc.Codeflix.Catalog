using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Category.Common;
using FC.Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;
using FC.Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.UpdateCategory;

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
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategory(Catalog.Domain.Entity.Category exampleCategory, UpdateCategoryInput input)
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWork = _fixture.GetUnitOfWork();
        
        repositoryMock.Setup(x =>
            x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleCategory);
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory.UpdateCategory(repositoryMock.Object, unitOfWork.Object);

        CategoryModelOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(exampleCategory.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be((bool) input.IsActive!);
        
        repositoryMock.Verify(x =>
            x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        repositoryMock.Verify(x =>
            x.Update(exampleCategory, It.IsAny<CancellationToken>()), Times.Once);
        
        unitOfWork.Verify(x => 
            x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact(DisplayName = nameof(ThrowsExceptionWhenCategoryIsNotFound))]
    [Trait("Application ", "UpdateCategory - UseCases")]
    public async Task ThrowsExceptionWhenCategoryIsNotFound()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWork = _fixture.GetUnitOfWork();
        var input = _fixture.GetValidInput();
        repositoryMock.Setup(x =>
                x.Get(input.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"category '{input.Id} not found'"));
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory.UpdateCategory(repositoryMock.Object, unitOfWork.Object);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>();
        repositoryMock.Verify(x => 
            x.Get(input.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Theory(DisplayName = nameof(UpdateCategoryWithoutProvidingIsActive))]
    [Trait("Application ", "UpdateCategory - UseCases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task UpdateCategoryWithoutProvidingIsActive(Catalog.Domain.Entity.Category exampleCategory, UpdateCategoryInput exampleInput)
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWork = _fixture.GetUnitOfWork();
        
        repositoryMock.Setup(x =>
                x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleCategory);
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory.UpdateCategory(repositoryMock.Object, unitOfWork.Object);
        var input = new UpdateCategoryInput(exampleInput.Id, exampleCategory.Name, exampleCategory.Description);
        
        CategoryModelOutput output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(exampleCategory.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);
        
        repositoryMock.Verify(x =>
            x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        repositoryMock.Verify(x =>
            x.Update(exampleCategory, It.IsAny<CancellationToken>()), Times.Once);
        
        unitOfWork.Verify(x => 
            x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = "")]
    [Trait("Application", "Update Category - UseCase")]
    [MemberData(nameof(UpdateCategoryTestDataGenerator.GetInvalidInputs), parameters: 12,
        MemberType = typeof(UpdateCategoryTestDataGenerator))]
    public async Task ThrowWhenCannotUpdateCategory(UpdateCategoryInput input, string expectedExceptionMessage)
    {
        var exampleCategory = _fixture.GetValidCategory();
        input.Id = exampleCategory.Id;
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWork = _fixture.GetUnitOfWork();
        
        repositoryMock.Setup(x =>
                x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleCategory);
        
        var useCase = new Catalog.Application.UseCases.Category.UpdateCategory.UpdateCategory(repositoryMock.Object, unitOfWork.Object);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<EntityValidationException>()
            .WithMessage(expectedExceptionMessage);
        
        repositoryMock.Verify(x => 
            x.Get(exampleCategory.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}