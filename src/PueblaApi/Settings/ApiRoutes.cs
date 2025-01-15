namespace PueblaApi.Settings
{
    record ApiRouteSettings
    {
        public const string Server = "http://localhost:5006";
        public const string Base = $"api/v1";
    }
    public record ApiControllerRoutes
    {
        public const string Authentication = $"{ApiRouteSettings.Base}/auth";
        public const string User = $"{ApiRouteSettings.Base}/users";
    }
}