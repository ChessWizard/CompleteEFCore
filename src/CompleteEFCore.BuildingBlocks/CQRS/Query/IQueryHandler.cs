using MediatR;

namespace CompleteEFCore.BuildingBlocks.CQRS.Query
{
    public interface IQueryHandler<in IQuery>
        : IQueryHandler<IQuery, Unit>
        where IQuery : IQuery<Unit>
    {
    }

    public interface IQueryHandler<in IQuery, TResponse>
        : IRequestHandler<IQuery, TResponse>
        where IQuery : IQuery<TResponse>
        where TResponse : notnull
    {
    }
}
