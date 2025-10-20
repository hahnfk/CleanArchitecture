using System.Windows.Input;
using CleanArchitecture.Presentation.Wpf.Commands;

namespace CleanArchitecture.Presentation.Wpf.Tests.TestHelpers;

public static class CommandTestExtensions
{
    /// <summary>
    /// Executes any ICommand in an awaitable way.
    /// - If it's your own AsyncRelayCommand, we call its ExecuteAsync directly.
    /// - Otherwise we run ICommand.Execute(...) on a thread-pool Task.
    /// </summary>
    public static Task ExecuteAsync(this ICommand? cmd, object? parameter = null)
    {
        if (cmd is null) throw new ArgumentNullException(nameof(cmd));

        // 1) Deine eigene AsyncRelayCommand (nicht generisch)
        if (cmd is AsyncRelayCommand mine)
            return mine.ExecuteAsync(parameter);

        // 2) Fallback: synchrones ICommand in Task ausführen
        return Task.Run(() => cmd.Execute(parameter));
    }
}
