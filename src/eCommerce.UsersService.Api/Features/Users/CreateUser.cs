using Carter;
using eCommerce.UsersService.Api.Abstractions.Messaging;
using eCommerce.UsersService.Api.Contracts.Users;
using eCommerce.UsersService.Api.Database;
using eCommerce.UsersService.Api.Entities;
using eCommerce.UsersService.Api.Shared.Bases;
using FluentValidation;
using Mapster;
using BC = BCrypt.Net.BCrypt;

namespace eCommerce.UsersService.Api.Features.Users;

public class CreateUser
{
    #region Command
    public sealed class Command : ICommand<bool>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    #endregion

    #region Validator
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .NotNull()
                .WithMessage("El nombre no puede ser nulo ni vacío.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .NotNull()
                .WithMessage("El apellido no puede ser nulo ni vacío.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .NotNull()
                .EmailAddress()
                .WithMessage("El email debe ser válido.");
        }
    }
    #endregion

    #region Handler
    internal sealed class Handler(ApplicationDbContext context, 
        HandlerExecutor executor) : ICommandHandler<Command, bool>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HandlerExecutor _executor = executor;

        public async Task<BaseResponse<bool>> Handle(Command command, 
            CancellationToken cancellationToken)
        {
            return await _executor.ExecuteAsync(
                command,
                () => CreateUserAsync(command, cancellationToken),
                cancellationToken
                );
        }

        private async Task<BaseResponse<bool>> CreateUserAsync(Command command,
            CancellationToken cancellationToken)
        {
            var response = new BaseResponse<bool>();

            try
            {
                var user = command.Adapt<User>();
                user.Password = BC.HashPassword(command.Password);

                _context.Add(user);

                var recordsAffected = await _context.SaveChangesAsync(cancellationToken);

                response.IsSuccess = true;
                response.Message = "Se registró correctamente.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Ocurrió un error al registrar el usuario: {ex.Message}";
            }

            return response;
        }
    }
    #endregion

    #region Endpoint
    public class CreateUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/users/register", async (
                    RegisterRequest request,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
            {
                var command = request.Adapt<Command>();
                var response = await dispatcher
                .Dispatch<Command, bool>(command, cancellationToken);

                return Results.Ok(response);
            })
            .WithName("RegisterUser")
            .WithTags("Users")
            .WithSummary("Registrar un nuevo usuario")
            .WithDescription("Crea un nuevo usuario en el sistema con los datos proporcionados. La contraseña será encriptada automáticamente.")
            .Produces<BaseResponse<bool>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Registrar un nuevo usuario";
                operation.Description = "Endpoint para crear un nuevo usuario en el sistema eCommerce. " +
                    "La contraseña se encripta automáticamente usando BCrypt antes de almacenarse.";
                return operation;
            });
        }
    }
    #endregion
}
