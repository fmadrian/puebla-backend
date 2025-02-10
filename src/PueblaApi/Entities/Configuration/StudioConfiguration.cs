using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PueblaApi.Entities.Configuration;

public class StudioConfiguration : IEntityTypeConfiguration<Studio>
{
    public void Configure(EntityTypeBuilder<Studio> builder)
    {
        // Set start value on ID column to avoid PK conflicts with seeded data.
        builder.Property(item => item.Id).HasIdentityOptions(startValue: 30);
    }
}