using FC.Codeflix.Catalog.Application.UseCases.Genre.Common;
using MediatR;

namespace FC.Codeflix.Catalog.Application.UseCases.Genre.CreateGenre;

public class CreateGenreInput : IRequest<GenreModelOutput>
{
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public List<Guid>? CategoryIds { get; set; }

    public CreateGenreInput(string name, bool isActive, List<Guid>? categoryIds = null)
    {
        Name = name;
        IsActive = isActive;
        CategoryIds = categoryIds;
    }
}