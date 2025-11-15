namespace eCommerce.UsersService.Api.Shared.Behaviors;

public interface IValidationService
{
    Task ValidateAsync<T>(T request, CancellationToken cancellationToken = default);
}
