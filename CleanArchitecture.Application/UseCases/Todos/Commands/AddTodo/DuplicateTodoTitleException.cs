
namespace CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo
{
    [Serializable]
    internal class DuplicateTodoTitleException : Exception
    {
        public DuplicateTodoTitleException()
        {
        }

        public DuplicateTodoTitleException(string? message) : base(message)
        {
        }

        public DuplicateTodoTitleException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}