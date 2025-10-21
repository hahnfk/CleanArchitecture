namespace CleanArchitecture.Presentation.Wpf.ViewModels;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using CleanArchitecture.Presentation.Wpf.Commands;
using CleanArchitecture.Presentation.Wpf.Models;
using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo;
using CleanArchitecture.Application.UseCases.Todos.Queries.ListTodos;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;

/// <summary>
/// ViewModel for Todos with inline add/edit/delete, decoupled from concrete handlers
/// by using IUseCase&lt;TRequest, TResponse&gt; interfaces.
/// </summary>
public sealed class TodosViewModel : INotifyPropertyChanged
{
    private readonly IUseCase<AddTodoRequest, AddTodoResponse> _addUseCase;
    private readonly IUseCase<Unit, ListTodosResponse> _listUseCase;
    private readonly IUseCase<CompleteTodoRequest, Unit> _completeUseCase;
    private readonly IUseCase<ReopenTodoRequest, Unit> _reopenUseCase;
    private readonly IUseCase<DeleteTodoRequest, Unit> _deleteUseCase;
    private readonly IUseCase<RenameTodoRequest, Unit>? _renameUseCase;

    public TodosViewModel(
        IUseCase<AddTodoRequest, AddTodoResponse> add,
        IUseCase<Unit, ListTodosResponse> list,
        IUseCase<CompleteTodoRequest, Unit> complete,
        IUseCase<ReopenTodoRequest, Unit> reopen,
        IUseCase<DeleteTodoRequest, Unit> delete,
        IUseCase<RenameTodoRequest, Unit>? rename = null)
    {
        _addUseCase = add;
        _listUseCase = list;
        _completeUseCase = complete;
        _reopenUseCase = reopen;
        _deleteUseCase = delete;
        _renameUseCase = rename;

        Items = new ObservableCollection<TodoModel>();
        Items.CollectionChanged += OnItemsCollectionChanged;

        // Commands
        AddInlineRowCommand = new AsyncRelayCommand(async _ => await AddInlineRowAsync());
        StartEditCommand = new AsyncRelayCommand(async p => await StartEditAsync(p as TodoModel));
        SaveNewCommand = new AsyncRelayCommand(async p => await SaveNewAsync(p as TodoModel), p => CanSaveNew(p as TodoModel));
        SaveEditCommand = new AsyncRelayCommand(async p => await SaveEditAsync(p as TodoModel), p => CanSaveEdit(p as TodoModel));
        CancelNewCommand = new AsyncRelayCommand(async p => await CancelNewAsync(p as TodoModel));
        CancelEditCommand = new AsyncRelayCommand(async p => await CancelEditAsync(p as TodoModel));
        DeleteCommand = new AsyncRelayCommand(async p => await DeleteAsync(p as TodoModel));
        CompleteCommand = new AsyncRelayCommand(async p => await CompleteAsync((p as TodoModel)?.Id));

        // initial load
        _ = RefreshAsync();
    }

    public ObservableCollection<TodoModel> Items { get; }

    // Exposed commands for XAML
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

    // ===== Data loading =====
    private async Task RefreshAsync()
    {
        var dto = await _listUseCase.Handle(Unit.Value);
        Items.CollectionChanged -= OnItemsCollectionChanged;
        Items.Clear();
        foreach (var t in dto.Items)
        {
            var m = new TodoModel(t.Id, t.Title, t.IsCompleted);
            m.PropertyChanged += OnRowPropertyChanged;
            Items.Add(m);
        }
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (TodoModel m in e.OldItems) m.PropertyChanged -= OnRowPropertyChanged;
        if (e.NewItems != null)
            foreach (TodoModel m in e.NewItems) m.PropertyChanged += OnRowPropertyChanged;
    }

    // Toggle completed directly via two-way binding
    private async void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TodoModel row) return;
        if (e.PropertyName == nameof(TodoModel.IsCompleted))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Id))
                    return; 

                if (row.IsCompleted)
                    await _completeUseCase.Handle(new CompleteTodoRequest { Id = row.Id });
                else
                    await _reopenUseCase.Handle(new ReopenTodoRequest { TodoId = row.Id });
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

    // ===== Inline add flow =====
    private Task AddInlineRowAsync()
    {
        var newRow = new TodoModel(id: "", title: "", isCompleted: false, isNew: true, isEditing: true);
        Items.Insert(0, newRow);
        return Task.CompletedTask;
    }

    private bool CanSaveNew(TodoModel? m)
        => m is { IsNew: true } && !string.IsNullOrWhiteSpace(m.EditableTitle);

    private async Task SaveNewAsync(TodoModel? m)
    {
        if (m is null) return;

        var res = await _addUseCase.Handle(new AddTodoRequest { Title = m.EditableTitle });
        // Finalize the inline row to a persisted row
        m.Id = res.Id;
        m.Title = res.Title;
        m.IsNew = false;
        m.IsEditing = false;
        m.EditableTitle = string.Empty;
    }

    private Task CancelNewAsync(TodoModel? m)
    {
        if (m is null) return Task.CompletedTask;
        if (m.IsNew) Items.Remove(m);
        return Task.CompletedTask;
    }

    // ===== Inline edit flow =====
    private Task StartEditAsync(TodoModel? m)
    {
        if (m is null || m.IsNew) return Task.CompletedTask;
        m.EditableTitle = m.Title;
        m.IsEditing = true;
        return Task.CompletedTask;
    }

    private bool CanSaveEdit(TodoModel? m)
        => m is { IsNew: false, IsEditing: true } &&
           !string.IsNullOrWhiteSpace(m.EditableTitle) &&
           m.EditableTitle != m.Title;

    private async Task SaveEditAsync(TodoModel? m)
    {
        if (m is null || _renameUseCase is null) return;

        await _renameUseCase.Handle(new RenameTodoRequest { TodoId = m.Id, NewTitle = m.EditableTitle });
        m.Title = m.EditableTitle;
        m.IsEditing = false;
        m.EditableTitle = string.Empty;
    }

    private Task CancelEditAsync(TodoModel? m)
    {
        if (m is null) return Task.CompletedTask;
        m.IsEditing = false;
        m.EditableTitle = string.Empty;
        return Task.CompletedTask;
    }

    // ===== Delete =====
    private async Task DeleteAsync(TodoModel? m)
    {
        if (m is null || string.IsNullOrWhiteSpace(m.Id)) return;
        await _deleteUseCase.Handle(new DeleteTodoRequest { TodoId = m.Id });
        Items.Remove(m);
    }

    // ===== Complete via explicit command (optional; UI kann auch 2-way Toggle nutzen) =====
    private async Task CompleteAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        await _completeUseCase.Handle(new CompleteTodoRequest { Id = id });

        var row = Items.FirstOrDefault(x => x.Id == id);
        if (row is not null) row.IsCompleted = true;
    }
}
