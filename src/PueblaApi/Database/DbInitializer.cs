using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PueblaApi.Database.Seedings;
using PueblaApi.Entities;
using PueblaApi.Settings;

namespace PueblaApi.Database
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            // We need to access to the DBContext service before the application starts.
            // Therefore, we need to create a scope.
            using var scope = app.Services.CreateScope(); // 'using' allows the framework to dispose of any of the services we've used 
                                                          // inside the scope we created once we're finished with this function.

            // From the application scopes, get DbContext, user and role managers.
            var services = scope.ServiceProvider;
            ApplicationDbContext context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            AdminUserConfiguration adminUserConfiguration = scope.ServiceProvider.GetService<AdminUserConfiguration>();

            // Creates the database (if it doesn't exist, and applies pending migrations)
            // context.Database.Migrate();

            // Seeds roles and administrator user.
            await UserRoleSeeding.Initialize(userManager, roleManager, adminUserConfiguration!);
            // Store categories and studios in database first.
            // Otherwise, we won't be able to seed movies.
            CategorySeeding.SeedData(context!);
            StudioSeeding.SeedData(context!);
            context!.SaveChanges();
            MovieSeeding.SeedData(context!);
            context!.SaveChanges();
        }
    }
}
