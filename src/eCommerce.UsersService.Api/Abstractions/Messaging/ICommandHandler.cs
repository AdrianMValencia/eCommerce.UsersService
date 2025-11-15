using eCommerce.UsersService.Api.Shared.Bases;

namespace eCommerce.UsersService.Api.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<BaseResponse<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
