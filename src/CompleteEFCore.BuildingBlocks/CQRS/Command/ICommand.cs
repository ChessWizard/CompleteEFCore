using MediatR;
using CompleteEFCore.BuildingBlocks.Result;

namespace CompleteEFCore.BuildingBlocks.CQRS.Command
{
    public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseRequest
    {
    }

    public interface ICommand : ICommand<Result<Unit>>, IBaseRequest
    {
    }
}
