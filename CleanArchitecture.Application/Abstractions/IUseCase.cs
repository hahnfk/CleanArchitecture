using CleanArchitecture.Application.Abstractions.ROP;

namespace CleanArchitecture.Application.Abstractions;

public interface IUseCase<in TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken ct = default);
}
