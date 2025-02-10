using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PueblaApi.Entities.Configuration;
/**
       Class where we define the additional configuration for the entity.
   **/
public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        // Set start value on ID column to avoid PK conflicts with seeded data.
        builder.Property(item => item.Id).HasIdentityOptions(startValue: 200);
    }
}