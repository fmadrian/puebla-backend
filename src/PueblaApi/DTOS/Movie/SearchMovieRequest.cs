using System.ComponentModel.DataAnnotations;
using PueblaApi.DTOS.Base;


namespace PueblaApi.DTOS.Movie;

public class SearchMovieRequest : SearchRequest
{

#nullable enable

    public string? Q { get; set; } // Q = query.
    public List<long>? Categories { get; set; } // Categories ID (optional).
    public long? Studio { get; set; } // Studio ID.

#nullable disable
}