namespace Domain.Entities
{
    /// <summary>
    /// Aggregate root representing a task.
    /// </summary>
    public sealed class TaskItem
    {
        public TaskItem(TaskId id, string title)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Title must not be empty.") : title.Trim();
            IsCompleted = false;
        }

        public TaskId Id { get; }
        public string Title { get; private set; }
        public bool IsCompleted { get; private set; }

        public void Rename(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title must not be empty.");
            Title = newTitle.Trim();
        }

        public TaskCompletedDomainEvent Complete()
        {
            if (IsCompleted) return TaskCompletedDomainEvent.None(Id);
            IsCompleted = true;
            return new TaskCompletedDomainEvent(Id);
        }
    }
}
