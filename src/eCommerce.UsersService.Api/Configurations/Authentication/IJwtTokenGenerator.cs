using eCommerce.UsersService.Api.Entities;

namespace eCommerce.UsersService.Api.Configurations.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
