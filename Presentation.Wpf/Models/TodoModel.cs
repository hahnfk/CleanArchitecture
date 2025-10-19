namespace CleanArchitecture.Presentation.Wpf.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// UI row model with inline editing state.
/// </summary>
public sealed class TodosModel : INotifyPropertyChanged
{
    private string _id;
    private string _title;
    private bool _isCompleted;

    private bool _isNew;
    private bool _isEditing;
    private string _editableTitle = string.Empty;

    public TodosModel(string id, string title, bool isCompleted, bool isNew = false, bool isEditing = false)
    {
        _id = id;
        _title = title;
        _isCompleted = isCompleted;
        _isNew = isNew;
        _isEditing = isEditing;
        _editableTitle = isEditing ? title : string.Empty;
    }

    public string Id { get => _id; set { _id = value; OnPropertyChanged(); } }
    public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
    public bool IsCompleted { get => _isCompleted; set { _isCompleted = value; OnPropertyChanged(); } }

    /// <summary>True for a temporary, unsaved row.</summary>
    public bool IsNew { get => _isNew; set { _isNew = value; OnPropertyChanged(); } }

    /// <summary>True if this row is being edited inline.</summary>
    public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

    /// <summary>Working buffer for inline editing.</summary>
    public string EditableTitle { get => _editableTitle; set { _editableTitle = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
