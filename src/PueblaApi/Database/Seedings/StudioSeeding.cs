using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PueblaApi.Database.Data.Seedings;
using PueblaApi.Entities;

namespace PueblaApi.Database.Seedings;

public class StudioSeeding
{
    public static void Seed(EntityTypeBuilder<Studio> builder)
    {
        // Seed the ingredients.
        builder.HasData(DefaultData.Studios);
    }
    public static void SeedData(ApplicationDbContext context)
    {
        if (context.Studios.Any())
            return;

        context.AddRange(DefaultData.Studios);
    }
}
