namespace CleanArchitecture.Presentation.Wpf.Models;

/// <summary>
/// UI-facing projection (presentation model) used by the View for data binding.
/// Not a domain entity and not persisted.
/// </summary>
public sealed record TodoModel(string Id, string Title, bool IsCompleted);
