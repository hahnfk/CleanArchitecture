using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CleanArchitecture.Presentation.Wpf.Commands;

/// <summary>
/// Minimal INotifyPropertyChanged base for MVVM
/// Provides SetProperty helper and OnPropertyChanged.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets the backing field and raises PropertyChanged if value actually changed.
    /// Returns true if the field changed; otherwise false.
    /// </summary>
    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises PropertyChanged for the given property.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
