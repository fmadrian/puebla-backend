using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PueblaApi.Entities.Configuration
{
    /**
        Class where we define the additional configuration for the entity.
    **/
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Unique constraint for "NationalId" field.
            builder.HasIndex(user => user.NationalId, "UK_AspNetUsers_NationalId").IsUnique();
        }
    }
}