namespace CleanArchitecture.Application.Abstractions;

/// <summary>
/// Represents 'no value' for request/response when using IUseCase.
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
