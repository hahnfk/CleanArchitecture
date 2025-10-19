using FakeItEasy;

using CleanArchitecture.Presentation.Wpf.ViewModels;
using CleanArchitecture.Application.UseCases.Todos.Queries.ListTasks;
using CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;
using CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTask;
using CleanArchitecture.Presentation.Wpf.Tests.TestHelpers;
using CleanArchitecture.Application.Abstractions;

namespace CleanArchitecture.Presentation.Wpf.Tests.Todos;

public sealed class TodosViewModelTests
{
    private static TodosViewModel CreateVmWithList(
        List<ListTodosResponse.TodoDto> seed,
        out IUseCase<AddTodoRequest, AddTodoResponse> add,
        out IUseCase<Unit, ListTodosResponse> list,
        out IUseCase<CompleteTodoRequest, Unit> complete,
        out IUseCase<ReopenTodoRequest, Unit> reopen,
        out IUseCase<DeleteTodoRequest, Unit> delete,
        out IUseCase<RenameTodoRequest, Unit> rename)
    {
        add = A.Fake<IUseCase<AddTodoRequest, AddTodoResponse>>();
        list = A.Fake<IUseCase<Unit, ListTodosResponse>>();
        complete = A.Fake<IUseCase<CompleteTodoRequest, Unit>>();
        reopen = A.Fake<IUseCase<ReopenTodoRequest, Unit>>();
        delete = A.Fake<IUseCase<DeleteTodoRequest, Unit>>();
        rename = A.Fake<IUseCase<RenameTodoRequest, Unit>>();

        A.CallTo(list)
            .WithReturnType<Task<ListTodosResponse>>()
            .Returns(Task.FromResult(new ListTodosResponse { Items = seed }));

        return new TodosViewModel(add, list, complete, reopen, delete, rename);
    }


    [Fact]
    public async Task ctor_Loads_Items_From_Query()
    {
        // Arrange
        var seed = new List<ListTodosResponse.TodoDto>
        {
            new("1", "A", false),
            new("2", "B", true)
        };

        // Act
        var vm = CreateVmWithList(seed, out _, out _, out _, out _, out _, out _);

        // Assert
        // ggf. minimal warten, falls dein ctor async lädt:
        await Task.Yield();
        Assert.Equal(2, vm.Items.Count);
        Assert.Equal("A", vm.Items [0].Title);
        Assert.Equal("B", vm.Items [1].Title);
        Assert.True(vm.Items [1].IsCompleted);
    }

    [Fact]
    public async Task AddInlineRow_Inserts_New_Editable_Row_At_Top()
    {
        // Arrange
        var vm = CreateVmWithList(new(), out _, out _, out _, out _, out _, out _);

        // Act
        await vm.AddInlineRowCommand.ExecuteAsync();

        // Assert
        Assert.True(vm.Items.Count >= 1);
        var row = vm.Items.First();
        Assert.True(row.IsNew);
        Assert.True(row.IsEditing);
        Assert.Equal("", row.Id);
        Assert.Equal("", row.EditableTitle);
    }

    [Fact]
    public async Task SaveNew_Calls_Add_Handler_And_Finalizes_Row()
    {
        // Arrange
        var vm = CreateVmWithList(new(), out var add, out _, out _, out _, out _, out _);
        await vm.AddInlineRowCommand.ExecuteAsync();
        var row = vm.Items.First();
        row.EditableTitle = "New Todo";

        A.CallTo(() => add.Handle(A<AddTodoRequest>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new AddTodoResponse { Id = "42", Title = "New Todo" }));

        // Act
        await vm.SaveNewCommand.ExecuteAsync(row);

        // Assert
        A.CallTo(() => add.Handle(
                A<AddTodoRequest>.That.Matches(r => r.Title == "New Todo"),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        Assert.False(row.IsNew);
        Assert.False(row.IsEditing);
        Assert.Equal("42", row.Id);
        Assert.Equal("New Todo", row.Title);
        Assert.Equal("", row.EditableTitle);
    }

    [Fact]
    public async Task StartEdit_And_SaveEdit_Renames_Title_Via_Handler()
    {
        // Arrange
        var seed = new List<ListTodosResponse.TodoDto>
        {
            new("99", "Old", false)
        };

        var vm = CreateVmWithList(seed, out _, out _, out _, out _, out _, out var rename);
        await Task.Yield(); // ctor-load durchlaufen lassen

        var row = vm.Items.Single();
        await vm.StartEditCommand.ExecuteAsync(row);
        row.EditableTitle = "New";

        // Rename-UseCase: IUseCase<RenameTodoRequest, Unit> -> Unit.Value zurückgeben
        A.CallTo(() => rename.Handle(
                A<RenameTodoRequest>.That.Matches(r => r.TodoId == "99" && r.NewTitle == "New"),
                A<CancellationToken>._))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        await vm.SaveEditCommand.ExecuteAsync(row);

        // Assert
        A.CallTo(() => rename.Handle(A<RenameTodoRequest>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        Assert.False(row.IsEditing);
        Assert.Equal("New", row.Title);
        Assert.Equal("", row.EditableTitle);
    }

    [Fact]
    public async Task Delete_Removes_Row_And_Calls_Handler()
    {
        // Arrange
        var seed = new List<ListTodosResponse.TodoDto>
        {
            new("1", "A", false)
        };
        var vm = CreateVmWithList(seed, out _, out _, out _, out _, out var delete, out _);
        await Task.Yield(); // ctor-load
        var row = vm.Items.Single();

        // Act
        await vm.DeleteCommand.ExecuteAsync(row);

        // Assert
        A.CallTo(() => delete.Handle(
                A<DeleteTodoRequest>.That.Matches(r => r.TodoId == "1"),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        Assert.Empty(vm.Items);
    }

    [Fact]
    public async Task Toggling_IsCompleted_Invokes_Complete_Or_Reopen()
    {
        // Arrange
        var seed = new List<ListTodosResponse.TodoDto>
        {
            new("7", "X", false)
        };
        var vm = CreateVmWithList(seed, out _, out _, out var complete, out var reopen, out _, out _);
        await Task.Yield(); // ctor-load
        var row = vm.Items.Single();

        // Act: set to completed -> should call Complete
        row.IsCompleted = true;

        // Assert
        A.CallTo(() => complete.Handle(
                A<CompleteTodoRequest>.That.Matches(r => r.Id == "7"),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        // Act: set back to not completed -> should call Reopen
        row.IsCompleted = false;

        // Assert
        A.CallTo(() => reopen.Handle(
                A<ReopenTodoRequest>.That.Matches(r => r.TodoId == "7"),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
