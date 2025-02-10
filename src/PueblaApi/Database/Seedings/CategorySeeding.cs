using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PueblaApi.Database.Data.Seedings;
using PueblaApi.Entities;

namespace PueblaApi.Database.Seedings;

public class CategorySeeding
{
    public static void Seed(EntityTypeBuilder<Category> builder)
    {
        // Seed the ingredients.
        builder.HasData(DefaultData.Categories);
    }
    public static void SeedData(ApplicationDbContext context)
    {
        if (context.Categories.Any())
            return;

        context.AddRange(DefaultData.Categories);
    }
}
