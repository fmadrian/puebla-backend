using Microsoft.AspNetCore.Identity;

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
        }
    }
}
