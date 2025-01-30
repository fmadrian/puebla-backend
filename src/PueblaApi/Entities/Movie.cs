using System;
using System.ComponentModel.DataAnnotations;

namespace PueblaApi.Entities;

public class Movie
{
    // PK
    public long Id { set; get; }
    [Required]
    public string Name { set; get; }
    [Required]
    public int ReleaseYear { set; get; }
    [Required]
    public long BoxOffice { set; get; }

    public string? ImageURL { set; get; }

    // Many to one (optional).
    public long? StudioId { set; get; }
    public Studio? Studio { set; get; }

    // Many to many.
    public List<Category> Categories { set; get; }

}
