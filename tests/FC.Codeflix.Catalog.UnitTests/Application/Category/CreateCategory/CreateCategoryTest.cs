using FC.Codeflix.Catalog.Application.UseCases.Category.CreateCategory;
using FC.Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.CreateCategory;

[Collection(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTest
{

    private readonly CreateCategoryTestFixture _fixture;

    public CreateCategoryTest(CreateCategoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = nameof(CreateCategory))]
    [Trait("Application", "Create Cagegory - Use Cases")]
    public async void CreateCategory()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWorkMock = _fixture.GetUnitOfWork();
        var useCase = new Catalog.Application.UseCases.Category.CreateCategory.CreateCategory(
            unitOfWorkMock.Object,
            repositoryMock.Object);

        var input = _fixture.GetInput();

        var output = await useCase.Handle(input, CancellationToken.None);
        
        repositoryMock.Verify(repository => 
            repository.Insert(It.IsAny<Catalog.Domain.Entity.Category>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(uow => 
            uow.Commit(It.IsAny<CancellationToken>()), Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive);
        (output.Id != Guid.Empty).Should().BeTrue();
        (output.CreatedAt != default(DateTime)).Should().BeTrue();
    }
    
    [Fact(DisplayName = nameof(CreateCategoryWithOnlyName))]
    [Trait("Application", "Create Cagegory - Use Cases")]
    public async void CreateCategoryWithOnlyName()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWorkMock = _fixture.GetUnitOfWork();
        var useCase = new Catalog.Application.UseCases.Category.CreateCategory.CreateCategory(
            unitOfWorkMock.Object,
            repositoryMock.Object);

        var input = new CreateCategoryInput(_fixture.GetValidCategoryName());

        var output = await useCase.Handle(input, CancellationToken.None);
        
        repositoryMock.Verify(repository => 
            repository.Insert(It.IsAny<Catalog.Domain.Entity.Category>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(uow => 
            uow.Commit(It.IsAny<CancellationToken>()), Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be("");
        output.IsActive.Should().BeTrue();
        (output.Id != Guid.Empty).Should().BeTrue();
        (output.CreatedAt != default(DateTime)).Should().BeTrue();
    }
    
    [Fact(DisplayName = nameof(CreateCategoryWithOnlyNameAndDescription))]
    [Trait("Application", "Create Cagegory - Use Cases")]
    public async void CreateCategoryWithOnlyNameAndDescription()
    {
        var repositoryMock = _fixture.GetCategoryRepository();
        var unitOfWorkMock = _fixture.GetUnitOfWork();
        var useCase = new Catalog.Application.UseCases.Category.CreateCategory.CreateCategory(
            unitOfWorkMock.Object,
            repositoryMock.Object);

        var input = new CreateCategoryInput(_fixture.GetValidCategoryName(), _fixture.GetValidCategoryDescription());

        var output = await useCase.Handle(input, CancellationToken.None);
        
        repositoryMock.Verify(repository => 
            repository.Insert(It.IsAny<Catalog.Domain.Entity.Category>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(uow => 
            uow.Commit(It.IsAny<CancellationToken>()), Times.Once);

        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().BeTrue();
        (output.Id != Guid.Empty).Should().BeTrue();
        (output.CreatedAt != default(DateTime)).Should().BeTrue();
    }

    [Theory(DisplayName = nameof(ThrowWhenCantInstantiateAggregate))]
    [Trait("Application", "Create Cagegory - Use Cases")]
    [MemberData(nameof(CreateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 100,
        MemberType = typeof(CreateCategoryTestDataGenerator))]
    public async void ThrowWhenCantInstantiateAggregate(
        CreateCategoryInput input, string exceptionMessage)
    {
        var useCase = new Catalog.Application.UseCases.Category.CreateCategory.CreateCategory(
            _fixture.GetUnitOfWork().Object,
            _fixture.GetCategoryRepository().Object);

        Func<Task> task = async () => await useCase.Handle(input, CancellationToken.None);

        await task.Should().ThrowAsync<EntityValidationException>().WithMessage(exceptionMessage);
    }

}