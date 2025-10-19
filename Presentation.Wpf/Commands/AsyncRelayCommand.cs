namespace CleanArchitecture.Presentation.Wpf.Commands;

using System;
using System.Threading.Tasks;
using System.Windows.Input;

/// <summary>
/// Minimal async ICommand implementation to keep MVVM lean without external libs.
/// Avoids blocking the UI thread and allows simple CanExecute gating.
/// </summary>
internal sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Func<object?, bool>? _canExecute;

    public AsyncRelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;

        // Optional: Auto-Requery an typische WPF-Ereignisse koppeln
        CommandManager.RequerySuggested += OnRequerySuggested;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter) => await _executeAsync(parameter);

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private void OnRequerySuggested(object? sender, EventArgs e) => RaiseCanExecuteChanged();
}
