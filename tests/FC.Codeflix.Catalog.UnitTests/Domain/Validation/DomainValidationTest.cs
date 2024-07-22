using Bogus;
using FC.Codeflix.Catalog.Domain.Exceptions;
using Xunit;
using FC.Codeflix.Catalog.Domain.Validation;
using FluentAssertions;
using Random = System.Random;

namespace FC.Codeflix.Catalog.UnitTests.Domain.Validation;

public class DomainValidationTest
{
    private Faker Faker { get; set; } = new ();
    
    
    // nao ser null
    [Fact(DisplayName = nameof(NotNullOk))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullOk()
    {
        var value = Faker.Commerce.ProductName();
        Action action = () => DomainValidation.NotNull(value, "Value");
        action.Should().NotThrow();
    }
    // nao ser null ou vazio
    [Fact(DisplayName = nameof(NotNullThrowsExceptionWhenNull))]
    [Trait("Domain", "DomainValidation - Validation")]
    public void NotNullThrowsExceptionWhenNull()
    {
        string? value = null;
        Action action = () => DomainValidation.NotNull(value, "Value");
        action.Should().Throw<EntityValidationException>().WithMessage("Value should not be null");
    }

    [Theory(DisplayName = nameof(NotNullOrEmptyThrowsExceptionWhenNull))]
    [Trait("Domain", "DomainValidation = Validation")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NotNullOrEmptyThrowsExceptionWhenNull(string? target)
    {
        Action action = () => DomainValidation.NotNullOrEmpty(target, "fieldName");
        action.Should().Throw<EntityValidationException>().WithMessage(("fieldName should not be null or empty"));
    }
    
    [Fact(DisplayName = nameof(NotNullOrEmptyOk))]
    [Trait("Domain", "DomainValidation = Validation")]
    public void NotNullOrEmptyOk()
    {
        string target = Faker.Commerce.ProductName();
        
        Action action = () => DomainValidation.NotNullOrEmpty(target, "fieldName");
        action.Should().NotThrow();
    }
    
    // tamanho minimo
    [Theory(DisplayName = nameof(MinLengthThrowWhenLess))]
    [Trait("Domain", "DomainValidation = Validation")]
    [MemberData(nameof(GetValuesSmallerThanMin), parameters: 5)]
    public void MinLengthThrowWhenLess(string target, int minLength)
    {
        Action action = () => DomainValidation.MinLength(target, minLength, "fieldName");
        action.Should().Throw<EntityValidationException>()
            .WithMessage($"fieldName should be at least {minLength} characters long");
    }
    
    [Theory(DisplayName = nameof(MinLengthOkWhenLess))]
    [Trait("Domain", "DomainValidation = Validation")]
    [MemberData(nameof(GetValuesGreaterThanMin), parameters: 5)]
    public void MinLengthOkWhenLess(string target, int minLength)
    {
        Action action = () => DomainValidation.MinLength(target, minLength, "fieldName");
        action.Should().NotThrow();
    }

    public static IEnumerable<object[]> GetValuesSmallerThanMin(int numberOfTests = 5)
    {
        var faker = new Faker();
        for (int i = 0; i < numberOfTests; i++)
        {
            var example = faker.Commerce.ProductName();
            var minLength = example.Length + (new Random().Next(1, 20));
            yield return new object[] {example, minLength};
        }
    }
    
    public static IEnumerable<object[]> GetValuesGreaterThanMin(int numberOfTests = 5)
    {
        var faker = new Faker();
        for (int i = 0; i < numberOfTests; i++)
        {
            var example = faker.Commerce.ProductName();
            var minLength = example.Length - (new Random().Next(0, example.Length));
            yield return new object[] {example, minLength};
        }
    }
    // tamanho maximo
    
    [Theory(DisplayName = nameof(MaxLengthThrowWhenGreater))]
    [Trait("Domain", "DomainValidation = Validation")]
    [MemberData(nameof(GetValuesGreaterThanMax), parameters: 5)]
    public void MaxLengthThrowWhenGreater(string target, int maxlength)
    {
        Action action = () => DomainValidation.MaxLength(target, maxlength, "fieldName");
        action.Should().Throw<EntityValidationException>()
            .WithMessage($"fieldName should be less or equal than {maxlength} characters long");
    }
    
    [Theory(DisplayName = nameof(MaxLengthOkWhenGreater))]
    [Trait("Domain", "DomainValidation = Validation")]
    [MemberData(nameof(GetValuesSmallerThanMax), parameters: 5)]
    public void MaxLengthOkWhenGreater(string target, int maxlength)
    {
        Action action = () => DomainValidation.MaxLength(target, maxlength, "fieldName");
        action.Should().NotThrow();
    }
    
    public static IEnumerable<object[]> GetValuesGreaterThanMax(int numberOfTests = 5)
    {
        var faker = new Faker();
        for (int i = 0; i < numberOfTests; i++)
        {
            var example = faker.Commerce.ProductName();
            var maxLength = example.Length - (new Random().Next(1, example.Length));
            yield return new object[] {example, maxLength};
        }
    }
    
    public static IEnumerable<object[]> GetValuesSmallerThanMax(int numberOfTests = 5)
    {
        var faker = new Faker();
        for (int i = 0; i < numberOfTests; i++)
        {
            var example = faker.Commerce.ProductName();
            var maxLength = example.Length + (new Random().Next(1, example.Length));
            yield return new object[] {example, maxLength};
        }
    }
}