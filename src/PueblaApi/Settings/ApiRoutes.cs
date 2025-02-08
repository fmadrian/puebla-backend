namespace PueblaApi.Settings
{
    record ApiRouteSettings
    {
        public const string Base = $"api/v1";
    }
    public record ApiControllerRoutes
    {
        public const string Authentication = $"{ApiRouteSettings.Base}/auth";
        public const string User = $"{ApiRouteSettings.Base}/users";
        public const string Movie = $"{ApiRouteSettings.Base}/movies";
        public const string Studio = $"{ApiRouteSettings.Base}/studios";

    }
}