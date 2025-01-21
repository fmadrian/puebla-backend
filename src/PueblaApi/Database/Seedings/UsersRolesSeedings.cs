using PueblaApi.Entities;
using PueblaApi.Settings;
using Microsoft.AspNetCore.Identity;

namespace PueblaApi.Database.Seedings;

public class UsersRolesSeedings
{
    /**
        Seeding data for users and roles works in a different way due to the interaction with Identity.

        This method is called each time the application starts, therefore, it is necessary to check that 
        roles and users don't exist before creating them.

        REMEMBER: Run the DB migration first (creates Identity related tables), then run the application to create roles and admin user.

        Solution implemented:
        https://stackoverflow.com/a/70736030
        https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/working-with-sql?view=aspnetcore-3.1&tabs=visual-studio#seed-the-database-1

    */
    public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AdminUserConfiguration adminUserConfiguration)
    {

        // 2. Create the roles.
        string[] roleNames = [ApiRoles.Admin, ApiRoles.Manager, ApiRoles.User];
        foreach (string rolename in roleNames)
        {
            // If the role doesn't exist, create it.
            if (!await roleManager.RoleExistsAsync(rolename))
            {
                await roleManager.CreateAsync(new IdentityRole(rolename));
            }
        }

        // Change the password after creating the admin user.

        // 3. Create new admin user.
        string email = adminUserConfiguration.Email, username = adminUserConfiguration.Username,
        password = adminUserConfiguration.Password;  // Requires at least 6 characters, 1 number, 1 uppercase and 1 lowercase), and 1 non-alphanumerical character.

        // Check if the administrator user exists.
        ApplicationUser admin = await userManager.FindByNameAsync(username);
        if (admin == null)
        {
            admin = new ApplicationUser()
            {
                Email = email,
                UserName = username,
                FirstName = "admin",
                LastName = "admin",
                NationalId = "0",
                IsEnabled = true,
                EmailConfirmed = true,
            };
            // If it doesn't exist, create administrator user and assign them the admin role.
            await userManager.CreateAsync(admin, password);
            await userManager.AddToRoleAsync(admin, roleNames[0]);
        }
        else
        {

            // If the user exists, and it doesn't have the role, assign it.
            if (!await userManager.IsInRoleAsync(admin, roleNames[0]))
            {
                await userManager.AddToRoleAsync(admin, roleNames[0]);
            }
        }
    }
}