using System;
using PueblaApi.DTOS.Movie;

namespace PueblaApi.DTOS.Studio;

public class StudioResponse
{
    public long Id { set; get; }
    public string Name { set; get; }
    public string Country { set; get; }
    public int FoundationYear { set; get; }
    public string ImageURL { set; get; }
    public List<MovieResponse?> Movies { set; get; }
}
