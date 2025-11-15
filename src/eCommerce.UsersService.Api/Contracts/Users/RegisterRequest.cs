namespace eCommerce.UsersService.Api.Contracts.Users;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);