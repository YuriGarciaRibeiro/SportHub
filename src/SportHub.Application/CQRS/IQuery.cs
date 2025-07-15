namespace Application.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
