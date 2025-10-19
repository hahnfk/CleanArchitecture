using System.Windows.Input;

namespace CleanArchitecture.Presentation.Wpf.Tests.TestHelpers;

public static class CommandTestExtensions
{
    public static Task ExecuteAsync(this ICommand cmd, object? parameter = null)
        => cmd switch
        {
            // Eigene AsyncRelayCommand? -> direkt async ausführen
            Commands.AsyncRelayCommand mine
                => mine.ExecuteAsync(parameter),

            // Fallback: sync ICommand, im Test-Thread ausführen
            _ => Task.Run(() => cmd.Execute(parameter))
        };
}
