namespace CleanArchitecture.Presentation.Wpf.ViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CleanArchitecture.Application.Tasks;
using CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;
using CleanArchitecture.Application.UseCases.Tasks.Commands.CompleteTask;
using CleanArchitecture.Application.UseCases.Tasks.Queries.ListTasks;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Presentation.Wpf.Commands;
using CleanArchitecture.Presentation.Wpf.Models;

/// <summary>
/// MVVM ViewModel for the TodosView (UserControl).
/// Orchestrates application use cases and exposes bindable state for the view.
/// </summary>
internal sealed class TodosViewModel : INotifyPropertyChanged
{
    private readonly AddTodoHandler _add;
    private readonly ListTodosHandler _list;
    private readonly CompleteTodoHandler _complete;
    private readonly RenameTodoHandler _rename;

    public TodosViewModel(AddTodoHandler add, ListTodosHandler list, CompleteTodoHandler complete, RenameTodoHandler rename)
    {
        _add = add;
        _list = list;
        _complete = complete;
        _rename = rename;

        Items = new ObservableCollection<TodoModel>();
        AddCommand = new AsyncRelayCommand(async _ => await AddAsync(), _ => !string.IsNullOrWhiteSpace(NewTitle));
        CompleteCommand = new AsyncRelayCommand(async id => await CompleteAsync(id as string));
        RenameCommand = new AsyncRelayCommand(async p =>
        {
            if (p is RenameParam rp && !string.IsNullOrWhiteSpace(rp.NewTitle))
            {
                await _rename.Handle(new RenameTodoRequest { TodoId = rp.TodoId, NewTitle = rp.NewTitle });
                await RefreshAsync();
            }
        });

        _ = RefreshAsync();
    }

    public ObservableCollection<TodoModel> Items { get; }
    public string NewTitle
    {
        get => _newTitle;
        set
        {
            if (_newTitle == value) return;
            _newTitle = value;
            OnPropertyChanged();

            // Re-evaluate AddCommand availability
            if (AddCommand is AsyncRelayCommand arc)
                arc.RaiseCanExecuteChanged();
        }
    }
    private string _newTitle = string.Empty;

    public ICommand AddCommand { get; }
    public ICommand CompleteCommand { get; }
    public ICommand RenameCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private async Task AddAsync()
    {
        await _add.Handle(new AddTodoRequest { Title = NewTitle });
        NewTitle = string.Empty;
        await RefreshAsync();
    }

    private async Task CompleteAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        await _complete.Handle(new CompleteTodoRequest { Id = id });
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        var dto = await _list.Handle();
        Items.Clear();
        foreach (var t in dto.Items)
            Items.Add(new TodoModel(t.Id, t.Title, t.IsCompleted));
    }
}
