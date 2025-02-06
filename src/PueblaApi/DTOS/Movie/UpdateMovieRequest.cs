using System;

namespace PueblaApi.DTOS.Movie;

public class UpdateMovieRequest
{
    public string? Name { set; get; }
    public int? ReleaseYear { set; get; }
    public long? BoxOffice { set; get; }
    public long? Studio { set; get; }
    public IFormFile? Image { set; get; }
    public List<long>? Categories { set; get; }
}
