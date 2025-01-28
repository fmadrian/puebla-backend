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
                //NationalId = "0",
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


        // 4. Seed normal users (3 managers and 7 users).
        await SeedUser(userManager, "john.doe@example.com", "johndoe", "John", "Doe", roleNames[1]);
        await SeedUser(userManager, "jane.smith@example.com", "janesmith", "Jane", "Smith", roleNames[1]);
        await SeedUser(userManager, "alex.johnson@example.com", "alexjohnson", "Alex", "Johnson", roleNames[1]);
        await SeedUser(userManager, "chris.taylor@example.com", "christaylor", "Chris", "Taylor", roleNames[2]);
        await SeedUser(userManager, "taylor.anderson@example.com", "tayloranderson", "Taylor", "Anderson", roleNames[2]);
        await SeedUser(userManager, "jordan.thomas@example.com", "jordanthomas", "Jordan", "Thomas", roleNames[2]);
        await SeedUser(userManager, "morgan.jackson@example.com", "morganjackson", "Morgan", "Jackson", roleNames[2]);
        await SeedUser(userManager, "riley.white@example.com", "rileywhite", "Riley", "White", roleNames[2]);
        await SeedUser(userManager, "parker.harris@example.com", "parkerharris", "Parker", "Harris", roleNames[2]);
        await SeedUser(userManager, "cameron.martin@example.com", "cameronmartin", "Cameron", "Martin", roleNames[2]);
    }
    /// <summary>
    /// Seeds users with the default 'user' role.
    /// </summary>
    /// <param name="email">User's email (automatically confirmed)</param>
    /// <param name="username">User's username</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="role">User's role (manager or user)</param>
    /// <returns></returns>
    private static async Task SeedUser(UserManager<ApplicationUser> userManager, string email, string username, string firstName, string lastName, string role = ApiRoles.User)
    {
        // Password for seeded users.
        string password = "User#123";
        ApplicationUser user = new ApplicationUser()
        {
            Email = email,
            UserName = username,
            FirstName = "admin",
            LastName = "admin",
            //NationalId = "0",
            IsEnabled = true,
            EmailConfirmed = true,
        };
        // If it doesn't exist, create administrator user and assign them the admin role.
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);
    }
}