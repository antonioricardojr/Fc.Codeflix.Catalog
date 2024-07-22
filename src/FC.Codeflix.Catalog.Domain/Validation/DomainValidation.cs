using FC.Codeflix.Catalog.Domain.Exceptions;

namespace FC.Codeflix.Catalog.Domain.Validation;

public class DomainValidation
{
    public static void NotNull(object? target, string fieldName)
    {
        if (target is null)
        {
            throw new EntityValidationException($"{fieldName} should not be null");
        }
    }

    public static void NotNullOrEmpty(string? target, string fieldname)
    {
        if (String.IsNullOrWhiteSpace(target))
        {
            throw new EntityValidationException($"{fieldname} should not be null or empty");
        }
    }

    public static void MinLength(string target, int minLength, string fieldname)
    {
        if (target.Length < minLength)
        {
            throw new EntityValidationException($"{fieldname} should be at least {minLength} characters long");
        }
    }

    public static void MaxLength(string target, int maxlength, string fieldname)
    {
        if (target.Length > maxlength)
        {
            throw new EntityValidationException($"{fieldname} should be less or equal than {maxlength} characters long");
        }
    }
}