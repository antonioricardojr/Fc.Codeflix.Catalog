namespace EndToEndTests.Api.Category.CreateCategory;

public class CreateCategoryApiTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs()
        {
            var fixture = new CreateCategoryApiTestFixture();
            var invalidInputList = new List<object[]>();
            var totalInvalidCases = 3;
            for (int i = 0; i < totalInvalidCases; i++)
            {
                switch (i % totalInvalidCases)
                {
                    case 0:
                        // nome não pode ser menos de 3 caracteres
                        var input0 = fixture.GetExampleInput();
                        input0.Name = fixture.GetInvalidNametooShort();
                        invalidInputList.Add(new object[]
                        {
                            input0,
                            $"Name should be at least 3 characters long"
                        });
                        break;
                    case 1:
                        // nome não pode ser maior do que 255 caracteres
                        var input1 = fixture.GetExampleInput();
                        input1.Name = fixture.GetInvalidNameTooLong();
                        invalidInputList.Add(new object[]
                        {
                            input1,
                            "Name should be less or equal than 255 characters long"
                        });
                        break;
                    case 2:
                        // description maior que 10_000 characters
                        var input3 = fixture.GetExampleInput();
                        input3.Description = fixture.GetInvalidInputTooLongDescription();
                        invalidInputList.Add(new object[]
                        {
                            input3,
                            "Description should be less or equal than 10000 characters long"
                        });
                        break;
                    default:
                        // nome não pode ser nulo
                        var input4 = fixture.GetExampleInput();
                        input4.Name = null;
                        invalidInputList.Add(new object[]
                        {
                            null,
                            "Name should not be null or empty"
                        });
                        break;       
                }
            }
            return invalidInputList;
        }

}