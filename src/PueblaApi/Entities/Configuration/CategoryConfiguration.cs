using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PueblaApi.Entities.Configuration;


/**
       Class where we define the additional configuration for the entity.
   **/
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Set start value on ID column to avoid PK conflicts with seeded data.
        builder.Property(item => item.Id).HasIdentityOptions(startValue: 30);
    }
}
