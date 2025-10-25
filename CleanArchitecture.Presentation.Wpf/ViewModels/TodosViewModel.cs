using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;
using CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;
using CleanArchitecture.Application.UseCases.Todos.Queries.ListTodos;
using CleanArchitecture.Presentation.Wpf.Commands;
using CleanArchitecture.Presentation.Wpf.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace CleanArchitecture.Presentation.Wpf.ViewModels;

public sealed class TodosViewModel : ObservableObject
{
    private readonly IUseCase<Unit, ListTodosResponse> _listTodosUseCase;
    private readonly IUseCase<AddTodoRequest, AddTodoResponse> _addTodoUseCase;
    private readonly IUseCase<RenameTodoRequest, Unit> _renameTodoUseCase;
    private readonly IUseCase<DeleteTodoRequest, Unit> _deleteTodoUseCase;
    private readonly IUseCase<CompleteTodoRequest, Unit> _completeTodoUseCase;
    private readonly IUseCase<ReopenTodoRequest, Unit> _reopenTodoUseCase;

    public ObservableCollection<TodoModel> Items { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand StartEditCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveNewCommand { get; }
    public ICommand CancelNewCommand { get; }
    public ICommand ToggleCompletedCommand { get; }

    public TodosViewModel(
        IUseCase<Unit, ListTodosResponse> listTodosUseCase,
        IUseCase<AddTodoRequest, AddTodoResponse> addTodoUseCase,
        IUseCase<RenameTodoRequest, Unit> renameTodoUseCase,
        IUseCase<DeleteTodoRequest, Unit> deleteTodoUseCase,
        IUseCase<CompleteTodoRequest, Unit> completeTodoUseCase,
        IUseCase<ReopenTodoRequest, Unit> reopenTodoUseCase)
    {
        _listTodosUseCase = listTodosUseCase;
        _addTodoUseCase = addTodoUseCase;
        _renameTodoUseCase = renameTodoUseCase;
        _deleteTodoUseCase = deleteTodoUseCase;
        _completeTodoUseCase = completeTodoUseCase;
        _reopenTodoUseCase = reopenTodoUseCase;

        // Toolbar "Add": add an inline 'new row' (IsNew = true, IsEditing = true)
        AddCommand = new AsyncRelayCommand(_ => AddInlineNewRowAsync());

        // Actions column (existing item)
        StartEditCommand = new AsyncRelayCommand<TodoModel>(p => StartEditAsync(p), requireNonNullParameter: true);
        SaveEditCommand = new AsyncRelayCommand<TodoModel>(p => SaveEditAsync(p), p => CanSaveEdit(p), requireNonNullParameter: true);
        CancelEditCommand = new AsyncRelayCommand<TodoModel>(p => CancelEditAsync(p), requireNonNullParameter: true);
        DeleteCommand = new AsyncRelayCommand<TodoModel>(p => DeleteAsync(p), requireNonNullParameter: true);

        // Actions column (new inline row)
        SaveNewCommand = new AsyncRelayCommand<TodoModel>(p => SaveNewAsync(p), p => CanSaveNew(p), requireNonNullParameter: true);
        CancelNewCommand = new AsyncRelayCommand<TodoModel>(p => CancelNewAsync(p), requireNonNullParameter: true);

        // Optional: command-based toggle (UI can bind to this);
        // tests that set IsCompleted directly are handled via PropertyChanged wiring (see WireItem + OnItemPropertyChanged)
        ToggleCompletedCommand = new AsyncRelayCommand<TodoModel>(p => ToggleAsync(p), requireNonNullParameter: true);

        // initial load
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(CancellationToken ct = default)
    {
        var result = await _listTodosUseCase.Handle(Unit.Value, ct);
        if (result.IsSuccess)
        {
            Items.Clear();
            foreach (var dto in result.Value!.Items)
            {
                var vm = CreateItem(dto.Id, dto.Title, dto.IsCompleted);
                WireItem(vm);             // <<< ensure IsCompleted changes trigger use cases
                Items.Add(vm);
            }
        }
        else ShowErrors("Failed to load todos", result.Errors);
    }

    // --- New row ---
    private Task AddInlineNewRowAsync()
    {
        if (Items.Any(i => i.IsNew)) return Task.CompletedTask;
        var vm = CreateItem(id: "", title: "", completed: false, isNew: true);
        vm.IsEditing = true;
        WireItem(vm);                 // subscribe early; handler ignores IsNew items
        Items.Insert(0, vm);
        return Task.CompletedTask;
    }

    private bool CanSaveNew(TodoModel? vm)
        => vm is { IsNew: true } && !string.IsNullOrWhiteSpace(vm.EditableTitle);

    private async Task SaveNewAsync(TodoModel? vm, CancellationToken ct = default)
    {
        if (vm is null || !vm.IsNew) return;

        var result = await _addTodoUseCase.Handle(new AddTodoRequest { Title = vm.EditableTitle }, ct);
        if (result.IsSuccess)
        {
            var added = result.Value!;
            vm.Id = added.Id;
            vm.Title = added.Title;
            vm.IsNew = false;
            vm.IsEditing = false;
            vm.EditableTitle = string.Empty;

            WireItem(vm);            // <<< ensure now-persisted item is wired for toggle reactions
        }
        else ShowErrors("Failed to add todo", result.Errors);
    }

    private Task CancelNewAsync(TodoModel? vm)
    {
        if (vm is null || !vm.IsNew) return Task.CompletedTask;
        // optional: unsubscribe before remove
        vm.PropertyChanged -= OnItemPropertyChanged;
        Items.Remove(vm);
        return Task.CompletedTask;
    }

    // --- Edit existing ---
    private Task StartEditAsync(TodoModel? vm)
    {
        if (vm is null || vm.IsNew) return Task.CompletedTask;
        vm.EditableTitle = vm.Title;
        vm.IsEditing = true;
        return Task.CompletedTask;
    }

    private bool CanSaveEdit(TodoModel? vm)
        => vm is { IsNew: false, IsEditing: true } && !string.IsNullOrWhiteSpace(vm.EditableTitle);

    private async Task SaveEditAsync(TodoModel? vm, CancellationToken ct = default)
    {
        if (vm is null || vm.IsNew || string.IsNullOrWhiteSpace(vm.EditableTitle)) return;

        var result = await _renameTodoUseCase.Handle(new RenameTodoRequest
        {
            TodoId = vm.Id,
            NewTitle = vm.EditableTitle
        }, ct);

        if (result.IsSuccess)
        {
            vm.Title = vm.EditableTitle;
            vm.EditableTitle = string.Empty;
            vm.IsEditing = false;
        }
        else ShowErrors("Failed to rename todo", result.Errors);
    }

    private Task CancelEditAsync(TodoModel? vm)
    {
        if (vm is null || vm.IsNew) return Task.CompletedTask;
        vm.IsEditing = false;
        vm.EditableTitle = string.Empty;
        return Task.CompletedTask;
    }

    // --- Delete ---
    private async Task DeleteAsync(TodoModel? vm, CancellationToken ct = default)
    {
        if (vm is null) return;

        if (vm.IsNew)
        {
            vm.PropertyChanged -= OnItemPropertyChanged;
            Items.Remove(vm);
            return;
        }

        var result = await _deleteTodoUseCase.Handle(new DeleteTodoRequest { TodoId = vm.Id }, ct);
        if (result.IsSuccess)
        {
            vm.PropertyChanged -= OnItemPropertyChanged; // optional cleanup
            Items.Remove(vm);
        }
        else ShowErrors("Failed to delete todo", result.Errors);
    }

    // --- Toggle via command (UI path) ---
    private async Task ToggleAsync(TodoModel? vm, CancellationToken ct = default)
    {
        if (vm is null || vm.IsNew) return;

        if (!vm.IsCompleted)
        {
            var res = await _completeTodoUseCase.Handle(new CompleteTodoRequest { Id = vm.Id }, ct);
            if (res.IsSuccess) vm.IsCompleted = true;
            else ShowErrors("Failed to complete todo", res.Errors);
        }
        else
        {
            var res = await _reopenTodoUseCase.Handle(new ReopenTodoRequest { TodoId = vm.Id }, ct);
            if (res.IsSuccess) vm.IsCompleted = false;
            else ShowErrors("Failed to reopen todo", res.Errors);
        }
    }

    private static TodoModel CreateItem(string id, string title, bool completed, bool isNew = false)
        => new(id, title, completed, isNew);

    private void ShowErrors(string caption, IReadOnlyList<Error> errors)
    {
        var text = string.Join("\n", errors.Select(e => $"{e.Code}: {e.Message}{(string.IsNullOrWhiteSpace(e.Detail) ? "" : " (" + e.Detail + ")")}"));
        System.Windows.MessageBox.Show(text, caption);
    }

    // ---------- Wiring for tests & checkbox two-way updates ----------
    private void WireItem(TodoModel item)
    {
        // avoid double subscription
        item.PropertyChanged -= OnItemPropertyChanged;
        item.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TodoModel.IsCompleted))
            return;

        if (sender is not TodoModel vm || vm.IsNew)
            return;

        // Fire-and-forget so FakeItEasy records the call immediately (tests assert right after assignment)
        if (vm.IsCompleted)
        {
            var _ = _completeTodoUseCase.Handle(new CompleteTodoRequest { Id = vm.Id })
                .ContinueWith(t =>
                {
                    if (!t.Result.IsSuccess)
                    {
                        vm.IsCompleted = false; // revert UI state on failure
                    }
                });
        }
        else
        {
            var _ = _reopenTodoUseCase.Handle(new ReopenTodoRequest { TodoId = vm.Id })
                .ContinueWith(t =>
                {
                    if (!t.Result.IsSuccess)
                    {
                        vm.IsCompleted = true; // revert UI state on failure
                    }
                });
        }
    }
}
