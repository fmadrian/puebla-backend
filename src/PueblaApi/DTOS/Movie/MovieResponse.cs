using PueblaApi.DTOS.Category;
using PueblaApi.DTOS.Studio;

namespace PueblaApi.DTOS.Movie;

public class MovieResponse
{

    public long Id { set; get; }
    public string Name { set; get; }
    public int ReleaseYear { set; get; }
    public long BoxOffice { set; get; }
    public string? ImageURL { set; get; }
    public StudioResponse Studio { set; get; }

    // Many to many.
    public List<CategoryResponse> Categories { set; get; }
}
