using FC.Codeflix.Catalog.Application.UseCases.Category.UpdateCategory;

namespace FC.Codeflix.Catalog.UnitTests.Application.Category.UpdateCategory;

public class UpdateCategoryTestDataGenerator
{
    public static IEnumerable<object[]> GetCategoriesToUpdate(int times = 10)
    {
        var fixture = new UpdateCategoryTestFixture();
        
        for (int i = 0; i < times; i++)
        {
            var exampleCategory = fixture.GetValidCategory();
            var exampleInput = new UpdateCategoryInput(
                exampleCategory.Id,
                fixture.GetValidCategoryName(),
                fixture.GetValidCategoryDescription(),
                fixture.GetRandomBoolean()
            );

            yield return new object[]
            {
                exampleCategory, exampleInput
            };
        }
    }
    
    public static IEnumerable<object[]> GetInvalidInputs(int times = 12)
    {
        var fixture = new UpdateCategoryTestFixture();
        var invalidInputList = new List<object[]>();
        var totalInvalidCases = 3;
        for (int i = 0; i < times; i++)
        {
            switch (i % totalInvalidCases)
            {
                case 0:
                    // nome não pode ser menos de 3 caracteres
                    invalidInputList.Add(new object[]
                    {
                        fixture.GetInvalidInputShortName(),
                        $"Name should be at least 3 characters long"
                    });
                    break;
                case 1:
                    // nome não pode ser maior do que 255 caracteres
                    invalidInputList.Add(new object[]
                    {
                        fixture.GetInvalidInputTooLongName(),
                        "Name should be less or equal than 255 characters long"
                    });
                    break;
                case 2:
                    // description maior que 10_000 characters
                    invalidInputList.Add(new object[]
                    {
                        fixture.GetInvalidInputTooLongDescription(),
                        "Description should be less or equal than 10000 characters long"
                    });
                    break;
                default:
                    // nome não pode ser nulo
                    invalidInputList.Add(new object[]
                    {
                        fixture.InvalidInputNameNull(),
                        "Name should not be null or empty"
                    });
                    break;       
            }
        }
        return invalidInputList;
    }
}