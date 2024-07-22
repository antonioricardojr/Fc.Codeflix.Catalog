using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Category.DeleteCategory;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest
{
    private readonly DeleteCategoryTestFixture _fixture;

    public DeleteCategoryTest(DeleteCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Application", "DeleteCategory - UseCases")]
    public async Task DeleteCategory()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWorkMock = _fixture.GetUnitOfWork();
        var validCategoryExample = _fixture.GetValidCategory();

        repositoryMock.Setup(x =>
            x.Get(validCategoryExample.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validCategoryExample);
        
        var input = new DeleteCategoryInput(validCategoryExample.Id);

        var useCase = new Catalog.Application.UseCases.Category.DeleteCategory.DeleteCategory(repositoryMock.Object, unitOfWorkMock.Object);

        await useCase.Handle(input, CancellationToken.None);
        
        repositoryMock.Verify(x => 
            x.Get(validCategoryExample.Id, It.IsAny<CancellationToken>()), Times.Once);
        
        repositoryMock.Verify(x => 
        x.Delete(validCategoryExample, It.IsAny<CancellationToken>()), Times.Once);
        
        unitOfWorkMock.Verify(x => 
            x.Commit(It.IsAny<CancellationToken>()), Times.Once);
        
    }
    
    [Fact(DisplayName = nameof(ThrowsExceptionWhenCategoryIsNotFound))]
    [Trait("Application", "DeleteCategory - UseCases")]
    public async Task ThrowsExceptionWhenCategoryIsNotFound()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWorkMock = _fixture.GetUnitOfWork();
        var exampleGuid = Guid.NewGuid();

        repositoryMock.Setup(x =>
                x.Get(exampleGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category ''{exampleGuid} not found"));
        
        var input = new DeleteCategoryInput(exampleGuid);

        var useCase = new Catalog.Application.UseCases.Category.DeleteCategory.DeleteCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>();
        
        repositoryMock.Verify(x => 
            x.Get(exampleGuid, It.IsAny<CancellationToken>()), Times.Once);
    }
}