using EndToEndTests.Api.Category.Common;
using EndToEndTests.Base;

namespace EndToEndTests.Api.Genre.Common;

public class GenreBaseFixture : BaseFixture
{
    public GenrePersistence Persistence;
    public CategoryPersistence CategoryPersistence;

    public GenreBaseFixture() : base()
    {
        var dbContext = CreateDbContext();
        Persistence = new GenrePersistence(dbContext);
        CategoryPersistence = new CategoryPersistence(dbContext);
    }
}