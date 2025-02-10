using System;
using System.ComponentModel.DataAnnotations;

namespace PueblaApi.Entities;

public class Category
{
    public long Id { set; get; }
    [Required]
    public string Name { set; get; }

    // Many to many.
    public List<Movie> Movies { set; get; }

}
