using System;
using System.ComponentModel.DataAnnotations;

namespace PueblaApi.Entities;

public class Studio
{
    public long Id { set; get; }
    [Required]
    public string Name { set; get; }
    [Required]
    public string Country { set; get; }
    [Required]
    public int FoundationYear { set; get; }

    // One to many.
    public List<Movie> Movies { set; get; }
}
