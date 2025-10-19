// CleanArchitecture.Presentation.Wpf/Commands/AsyncRelayCommand.cs
namespace CleanArchitecture.Presentation.Wpf.Commands;

using System;
using System.Threading.Tasks;
using System.Windows.Input;

/// <summary>
/// Simple async-capable ICommand with optional CanExecute predicate.
/// Provides ExecuteAsync for tests and RaiseCanExecuteChanged for UI.
/// </summary>
internal class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Func<object?, bool>? _canExecute;

    public AsyncRelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;

        // Optional: lets WPF requery CanExecute on focus/keyboard changes
        try { CommandManager.RequerySuggested += (_, __) => RaiseCanExecuteChanged(); } catch { /* ok in headless tests */ }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter) => await _executeAsync(parameter);

    /// <summary>Explicit async execution; handy for unit tests.</summary>
    public Task ExecuteAsync(object? parameter) => _executeAsync(parameter);

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
