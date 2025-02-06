using System;
using PueblaApi.DTOS.Movie;

namespace PueblaApi.DTOS.Category;

public class CategoryResponse
{
    public long Id { set; get; }
    public string Name { set; get; }

    // Many to many.
    public List<MovieResponse>? Movies { set; get; }
}
