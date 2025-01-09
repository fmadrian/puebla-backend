namespace PueblaApi.Settings;

/**
    Record that defines the roles we are going to use in the application.
**/
public record ApiRoles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string User= "user";
}