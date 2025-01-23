namespace IPacientesApi.Dtos.User;

public class UserResponse
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // public string NationalId { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public bool IsEnabled { get; set; }

}
