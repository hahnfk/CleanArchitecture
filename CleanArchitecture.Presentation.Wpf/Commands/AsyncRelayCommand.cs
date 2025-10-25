using System.Windows.Input;

namespace CleanArchitecture.Presentation.Wpf.Commands
{
    /// <summary>
    /// An asynchronous ICommand implementation for WPF (.NET 8).
    /// Provides fire-and-forget semantics and optional async awaiting in tests.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Func<object?, bool>? _canExecute;

        /// <param name="executeAsync">The async method to execute.</param>
        /// <param name="canExecute">Optional predicate for enabling/disabling the command.</param>
        public AsyncRelayCommand(Func<object?, Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;

            try { CommandManager.RequerySuggested += (_, __) => RaiseCanExecuteChanged(); } catch { /* ignored */ }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter)
        {
            // Fire-and-forget ICommand pattern
            try
            {
                await _executeAsync(parameter).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Optional: log exceptions here or handle globally
                System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand exception: {ex}");
            }
        }

        /// <summary>
        /// Allows explicit awaiting in unit tests instead of fire-and-forget.
        /// </summary>
        public Task ExecuteAsync(object? parameter) => _executeAsync(parameter);

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// An asynchronous ICommand implementation with a strongly typed parameter.
    /// </summary>
    /// <typeparam name="T">The command parameter type.</typeparam>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _executeAsync;
        private readonly Func<T?, bool>? _canExecute;
        private readonly bool _requireNonNullParameter;

        /// <param name="executeAsync">The async method that receives a parameter of type T.</param>
        /// <param name="canExecute">Optional predicate for enabling/disabling the command.</param>
        /// <param name="requireNonNullParameter">
        /// If true, CanExecute returns false when parameter is null.
        /// </param>
        public AsyncRelayCommand(
            Func<T?, Task> executeAsync,
            Func<T?, bool>? canExecute = null,
            bool requireNonNullParameter = false)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
            _requireNonNullParameter = requireNonNullParameter;

            try { CommandManager.RequerySuggested += (_, __) => RaiseCanExecuteChanged(); } catch { /* ignored */ }
        }

        public bool CanExecute(object? parameter)
        {
            var cast = CastParameter(parameter);
            if (_requireNonNullParameter && cast is null) return false;
            return _canExecute?.Invoke(cast) ?? true;
        }

        public async void Execute(object? parameter)
        {
            var cast = CastParameter(parameter);
            if (_requireNonNullParameter && cast is null) return;

            try
            {
                await _executeAsync(cast).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand<{typeof(T).Name}> exception: {ex}");
            }
        }

        /// <summary>
        /// Allows explicit awaiting in unit tests instead of fire-and-forget.
        /// </summary>
        public Task ExecuteAsync(object? parameter) => _executeAsync(CastParameter(parameter));

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private static T? CastParameter(object? parameter)
        {
            // Fast path when parameter is already of the correct type
            if (parameter is T ok) return ok;

            if (parameter is null) return default;

            // Attempt basic convertible types (string->int, etc.)
            try
            {
                if (typeof(T).IsValueType && parameter is IConvertible)
                {
                    var converted = Convert.ChangeType(parameter, typeof(T));
                    return (T?)converted;
                }
            }
            catch
            {
                // ignore conversion failures
            }

            return default;
        }
    }
}
