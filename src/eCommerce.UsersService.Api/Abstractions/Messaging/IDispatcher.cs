using eCommerce.UsersService.Api.Shared.Bases;

namespace eCommerce.UsersService.Api.Abstractions.Messaging;

public interface IDispatcher
{
    Task<BaseResponse<TResponse>> Dispatch<TRequest, TResponse>(
        TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>;
}
