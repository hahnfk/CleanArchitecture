using System;

namespace CleanArchitecture.Domain.Entities
{
    /// <summary>
    /// Value object for task identity.
    /// </summary>
    public sealed class TaskId : IEquatable<TaskId>
    {
        public TaskId(Guid value) { Value = value; }
        public Guid Value { get; }

        public static TaskId New() => new TaskId(Guid.NewGuid());

        public bool Equals(TaskId other) => other != null && Value.Equals(other.Value);
        public override bool Equals(object obj) => Equals(obj as TaskId);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString("D");
    }
}
