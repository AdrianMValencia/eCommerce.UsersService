namespace eCommerce.UsersService.Api.Contracts.Users;

public class AuthenticationResponse
{
    public Guid UserID { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}
