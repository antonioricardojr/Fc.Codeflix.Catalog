using FC.Codeflix.Catalog.Domain.Entity;

namespace FC.Codeflix.Catalog.Infra.Data.EF.Models;

public class GenresCategories
{
    public Guid CategoryId { get; set; }
    public Guid GenreId { get; set; }
    public Category? Category { get; set; }
    public Genre? Genre { get; set; }

    public GenresCategories(Guid categoryId, Guid genreId)
    {
        CategoryId = categoryId;
        GenreId = genreId;
    }
}