using eCommerce.UsersService.Api.Shared.Bases;

namespace eCommerce.UsersService.Api.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<BaseResponse<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
