namespace PueblaApi.Database.Seedings;


using System.Globalization;
using PueblaApi.Database;
using PueblaApi.Database.Seedings;
using PueblaApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PueblaApi.Database.Data.Seedings;

public class MovieSeeding
{

    public static void Seed(EntityTypeBuilder<Movie> builder)
    {
        // Seed the data.
        builder.HasData(DefaultData.Categories);
    }
    public static void SeedData(ApplicationDbContext context)
    {
        if (context.Movies.Any())
            return;
        // Change te state of the related entities to 'Unchanged' so EF Core doesn't try to update them.   
        List<Movie> movies = DefaultData.Movies.Select(
            movie =>
            {
                // Change the state of the categories to 'Unchanged' so EF Core doesn't try to update the category.
                movie.Categories = movie.Categories.Select(category =>
                {
                    context.Entry(category).State = EntityState.Unchanged;
                    return category;
                }).ToList();

                // Change the state of the studio to 'Unchanged' so EF Core doesn't try to update the studio.
                if (movie.Studio != null)
                    context.Entry(movie.Studio).State = EntityState.Unchanged;



                return movie;
            }
        ).ToList();

        context.AddRange(movies);
    }
}
