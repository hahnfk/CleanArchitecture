namespace CleanArchitecture.Presentation.Wpf.ViewModels;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTask;
using CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;
using CleanArchitecture.Presentation.Wpf.Commands;
using CleanArchitecture.Presentation.Wpf.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CleanArchitecture.Application.UseCases.Todos.Commands;
using CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;
using CleanArchitecture.Application.UseCases.Todos.Queries.ListTasks;

/// <summary>
/// ViewModel for Todos with inline add/edit/delete.
/// </summary>
public sealed class TodosViewModel : INotifyPropertyChanged
{
    private readonly CompleteTodoHandler _complete;
    private readonly ReopenTodoHandler _reopen;
    private readonly ListTodosHandler _list;
    private readonly AddTodoHandler _add;
    private readonly DeleteTodoHandler _delete;
    private readonly RenameTodoHandler? _rename;

    public TodosViewModel(
        AddTodoHandler add,
        ListTodosHandler list,
        CompleteTodoHandler complete,
        ReopenTodoHandler reopen,
        DeleteTodoHandler delete,
        RenameTodoHandler? rename = null)
    {
        _add = add;
        _list = list;
        _complete = complete;
        _reopen = reopen;
        _delete = delete;
        _rename = rename;

        Items = new ObservableCollection<TodosModel>();
        Items.CollectionChanged += OnItemsCollectionChanged;

        // Commands
        AddInlineRowCommand = new AsyncRelayCommand(async _ => await AddInlineRowAsync());
        StartEditCommand = new AsyncRelayCommand(async p => await StartEditAsync(p as TodosModel));
        SaveNewCommand = new AsyncRelayCommand(async p => await SaveNewAsync(p as TodosModel), p => CanSaveNew(p as TodosModel));
        SaveEditCommand = new AsyncRelayCommand(async p => await SaveEditAsync(p as TodosModel), p => CanSaveEdit(p as TodosModel));
        CancelNewCommand = new AsyncRelayCommand(async p => await CancelNewAsync(p as TodosModel));
        CancelEditCommand = new AsyncRelayCommand(async p => await CancelEditAsync(p as TodosModel));
        DeleteCommand = new AsyncRelayCommand(async p => await DeleteAsync(p as TodosModel));
        CompleteCommand = new AsyncRelayCommand(async p => await CompleteAsync((p as TodosModel)?.Id));

        _ = RefreshAsync();
    }

    public ObservableCollection<TodosModel> Items { get; }

    private async Task RefreshAsync()
    {
        var dto = await _list.Handle();
        Items.CollectionChanged -= OnItemsCollectionChanged;
        Items.Clear();
        foreach (var t in dto.Items)
        {
            var m = new TodosModel(t.Id, t.Title, t.IsCompleted);
            m.PropertyChanged += OnRowPropertyChanged;
            Items.Add(m);
        }
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (TodosModel m in e.OldItems) m.PropertyChanged -= OnRowPropertyChanged;
        if (e.NewItems != null)
            foreach (TodosModel m in e.NewItems) m.PropertyChanged += OnRowPropertyChanged;
    }

    private async void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TodosModel row) return;
        if (e.PropertyName == nameof(TodosModel.IsCompleted))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Id))
                    return; // unsaved inline row – kein UseCase

                if (row.IsCompleted)
                    await _complete.Handle(new CompleteTodoRequest { Id = row.Id });
                else
                    await _reopen.Handle(new ReopenTodoRequest { TodoId = row.Id });
            }
            catch
            {
                // Rollback UI if app use case fails
                row.PropertyChanged -= OnRowPropertyChanged;
                row.IsCompleted = !row.IsCompleted;
                row.PropertyChanged += OnRowPropertyChanged;
                throw;
            }
        }
    }

    // AddInlineRow: setze neue Zeile standardmäßig auf IsCompleted = false
    private Task AddInlineRowAsync()
    {
        var newRow = new TodosModel(id: "", title: "", isCompleted: false, isNew: true, isEditing: true);
        Items.Insert(0, newRow);
        return Task.CompletedTask;
    }

    // Commands exposed to XAML
    public ICommand AddInlineRowCommand { get; }
    public ICommand StartEditCommand { get; }
    public ICommand SaveNewCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelNewCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CompleteCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private bool CanSaveNew(TodosModel? m) => m is { IsNew: true } && !string.IsNullOrWhiteSpace(m.EditableTitle);

    private async Task SaveNewAsync(TodosModel? m)
    {
        if (m is null) return;
        var res = await _add.Handle(new AddTodoRequest { Title = m.EditableTitle });
        // Update the inline row to be a real row
        m.Id = res.Id;
        m.Title = res.Title;
        m.IsNew = false;
        m.IsEditing = false;
        m.EditableTitle = string.Empty;
        // Optional: ensure consistent ordering; or call await RefreshAsync();
    }

    private Task CancelNewAsync(TodosModel? m)
    {
        if (m is null) return Task.CompletedTask;
        // Remove unsaved row
        if (m.IsNew) Items.Remove(m);
        return Task.CompletedTask;
    }

    // === Inline Edit ===
    private Task StartEditAsync(TodosModel? m)
    {
        if (m is null || m.IsNew) return Task.CompletedTask;
        m.EditableTitle = m.Title;
        m.IsEditing = true;
        return Task.CompletedTask;
    }

    private bool CanSaveEdit(TodosModel? m)
        => m is { IsNew: false, IsEditing: true } && !string.IsNullOrWhiteSpace(m.EditableTitle) && m.EditableTitle != m.Title;

    private async Task SaveEditAsync(TodosModel? m)
    {
        if (m is null || _rename is null) return;
        await _rename.Handle(new RenameTodoRequest { TodoId = m.Id, NewTitle = m.EditableTitle });
        m.Title = m.EditableTitle;
        m.IsEditing = false;
        m.EditableTitle = string.Empty;
    }

    private Task CancelEditAsync(TodosModel? m)
    {
        if (m is null) return Task.CompletedTask;
        m.IsEditing = false;
        m.EditableTitle = string.Empty;
        return Task.CompletedTask;
    }

    // === Delete ===
    private async Task DeleteAsync(TodosModel? m)
    {
        if (m is null || string.IsNullOrWhiteSpace(m.Id)) return;
        await _delete.Handle(new DeleteTodoRequest { TodoId = m.Id });
        Items.Remove(m);
    }

    // === Complete ===
    private async Task CompleteAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        await _complete.Handle(new CompleteTodoRequest { Id = id });
        // update row locally
        var row = Items.FirstOrDefault(x => x.Id == id);
        if (row is not null) row.IsCompleted = true;
    }
}
