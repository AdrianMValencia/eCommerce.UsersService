using Carter;
using eCommerce.UsersService.Api.Abstractions.Messaging;
using eCommerce.UsersService.Api.Configurations.Authentication;
using eCommerce.UsersService.Api.Contracts.Users;
using eCommerce.UsersService.Api.Database;
using eCommerce.UsersService.Api.Shared.Bases;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace eCommerce.UsersService.Api.Features.Auth;

public class Login
{
    #region Query
    public sealed class Query : IQuery<AuthenticationResponse>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
    #endregion

    #region Validator
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .NotNull()
                .EmailAddress()
                .WithMessage("El email debe ser válido.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .NotNull()
                .WithMessage("La contraseña no puede ser nula ni vacía.");
        }
    }
    #endregion

    #region Handler
    internal sealed class Handler(
        ApplicationDbContext context,
        HandlerExecutor executor,
        IJwtTokenGenerator jwtTokenGenerator)
        : IQueryHandler<Query, AuthenticationResponse>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HandlerExecutor _executor = executor;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

        public async Task<BaseResponse<AuthenticationResponse>> Handle(
            Query query, CancellationToken cancellationToken)
        {
            return await _executor.ExecuteAsync(
                query,
                () => LoginAsync(query, cancellationToken),
                cancellationToken);
        }

        private async Task<BaseResponse<AuthenticationResponse>> LoginAsync(
            Query query, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<AuthenticationResponse>();

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(
                    x => x.Email == query.Email, cancellationToken);

                if (user is null)
                {
                    response.IsSuccess = false;
                    response.Message = "El usuario no existe en la base de datos.";
                    return response;
                }

                if (!BC.Verify(query.Password, user.Password))
                {
                    response.IsSuccess = false;
                    response.Message = "La contraseña es incorrecta.";
                    return response;
                }

                var authToken = user.Adapt<AuthenticationResponse>();
                authToken.Token = _jwtTokenGenerator.GenerateToken(user);

                response.IsSuccess = true;
                response.Data = authToken;
                response.Message = "Token generado correctamente.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Ocurrió un error al procesar la solicitud: {ex.Message}";
            }

            return response;
        }
    }
    #endregion

    #region Endpoint
    public class LoginEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/auth/login", async (
                LoginRequest request,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                var query = request.Adapt<Query>();
                var response = await dispatcher
                .Dispatch<Query, AuthenticationResponse>(query, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("Login")
            .WithTags("Authentication")
            .WithSummary("Iniciar sesión")
            .WithDescription("Autentica un usuario y genera un token JWT para acceder a los recursos protegidos del sistema.")
            .Produces<BaseResponse<AuthenticationResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Iniciar sesión en el sistema";
                operation.Description = "Endpoint para autenticar usuarios existentes. " +
                    "Valida las credenciales (email y contraseña) y devuelve un token JWT " +
                    "junto con la información del usuario si la autenticación es exitosa.";
                return operation;
            });
        }
    }
    #endregion
}
