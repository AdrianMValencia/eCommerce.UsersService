using Carter;
using eCommerce.UsersService.Api.Abstractions.Messaging;
using eCommerce.UsersService.Api.Contracts.Users;
using eCommerce.UsersService.Api.Database;
using eCommerce.UsersService.Api.Shared.Bases;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.UsersService.Api.Features.Users;

public class GetUserById
{
    #region Query
    public sealed class Query : IQuery<UserResponse>
    {
        public Guid UserId { get; set; }
    }
    #endregion

    #region Handler
    internal sealed class Handler : IQueryHandler<Query, UserResponse>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<UserResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<UserResponse>();

            try
            {
                var user = await _context.Users.
                    FirstOrDefaultAsync(u => u.UserID == query.UserId, cancellationToken);

                if (user is null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                    return response;
                }

                var userResponse = user.Adapt<UserResponse>();

                response.IsSuccess = true;
                response.Data = userResponse;
                response.Message = "User retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"An error occurred while retrieving the user. {ex.Message}";
            }

            return response;
        }
    }
    #endregion

    #region Endpoint
    public class GetByIdUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/users/{userId}", async (
                Guid userId,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                //await Task.Delay(100);
                //throw new NotImplementedException();

                var query = new Query { UserId = userId };

                var response = await dispatcher
                    .Dispatch<Query, UserResponse>(query, cancellationToken);

                if (!response.IsSuccess)
                    return Results.NotFound(response);

                return Results.Ok(response);
            });
        }
    }
    #endregion
}
