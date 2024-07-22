using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Application.UseCases.Category.GetCategory;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.GetCategory;

[Collection(nameof(GetCategoryTestFixture))]
public class GetCategoryTest
{
    private readonly GetCategoryTestFixture _fixture;

    public GetCategoryTest(GetCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }


    [Fact(DisplayName = nameof(GetCategory))]
    [Trait("Application", "Get Category - Use cases")]
    public async void GetCategory()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var exampleCategory = _fixture.GetValidCategory();
        
        repositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exampleCategory);
        var input = new GetCategoryInput(exampleCategory.Id);
        var useCase = new Catalog.Application.UseCases.Category.GetCategory.GetCategory(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(x => 
            x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        
        output.Should().NotBeNull();
        output.Name.Should().Be(exampleCategory.Name);
        output.Description.Should().Be(exampleCategory.Description);
        output.IsActive.Should().Be(exampleCategory.IsActive);
        output.Id.Should().Be(exampleCategory.Id);
    }
    
    [Fact(DisplayName = nameof(NotFoundExceptionWhenCategoryDoesNotExist))]
    [Trait("Application", "Get Category - Use cases")]
    public async void NotFoundExceptionWhenCategoryDoesNotExist()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var exampleGuid = Guid.NewGuid();
        
        repositoryMock.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category {exampleGuid}' not found"));
        var input = new GetCategoryInput(exampleGuid);
        var useCase = new Catalog.Application.UseCases.Category.GetCategory.GetCategory(repositoryMock.Object);

        var task = async () 
            => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<NotFoundException>();
        repositoryMock.Verify(x => 
            x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}