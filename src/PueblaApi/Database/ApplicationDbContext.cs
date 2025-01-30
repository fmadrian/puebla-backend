using System.Collections.Generic;
using System.IO.Compression;
using System.Reflection.Emit;
using System.Reflection;
using System;
using Microsoft.EntityFrameworkCore;
using PueblaApi.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PueblaApi.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Entities

        public DbSet<EmailConfirmationCode> EmailConfirmationCodes { get; set; }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Studio> Studios { get; set; }
        public DbSet<Category> Categories { get; set; }


        #endregion
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        #region Configuration and seeding
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurations for each model/entity are defined in the configurations folder.
            // Applies configurations defined in the Entities/Configuration folder.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        #endregion

        #region Conventions
        //  Conventions
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
        }
        #endregion
    }
}
