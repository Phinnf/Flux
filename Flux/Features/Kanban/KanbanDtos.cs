using System;
using System.Collections.Generic;
using Flux.Domain.Entities;

namespace Flux.Features.Kanban
{
    public class KanbanBoardDto
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<KanbanColumnDto> Columns { get; set; } = new();

        public KanbanBoardDto() { }
        public KanbanBoardDto(Guid id, Guid workspaceId, string name, string? description, List<KanbanColumnDto> columns)
        {
            Id = id;
            WorkspaceId = workspaceId;
            Name = name;
            Description = description;
            Columns = columns;
        }
    }

    public class KanbanColumnDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<KanbanTaskDto> Tasks { get; set; } = new();

        public KanbanColumnDto() { }
        public KanbanColumnDto(Guid id, string name, int order, List<KanbanTaskDto> tasks)
        {
            Id = id;
            Name = name;
            Order = order;
            Tasks = tasks;
        }
    }

    public class KanbanTaskDto
    {
        public Guid Id { get; set; }
        public Guid ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Priority Priority { get; set; } = Priority.Low;
        public DateTime? DueDate { get; set; }
        public string? Color { get; set; }
        public int Order { get; set; }
        public bool IsArchived { get; set; }
        public List<KanbanSubtaskDto> Subtasks { get; set; } = new();
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUsername { get; set; }
        public string? AssignedAvatarUrl { get; set; }

        public KanbanTaskDto() { }
        public KanbanTaskDto(
            Guid id, Guid columnId, string title, string? description, Priority priority, 
            DateTime? dueDate, string? color, int order, bool isArchived, 
            List<KanbanSubtaskDto> subtasks, Guid? assignedUserId, string? assignedUsername, string? assignedAvatarUrl)
        {
            Id = id;
            ColumnId = columnId;
            Title = title;
            Description = description;
            Priority = priority;
            DueDate = dueDate;
            Color = color;
            Order = order;
            IsArchived = isArchived;
            Subtasks = subtasks;
            AssignedUserId = assignedUserId;
            AssignedUsername = assignedUsername;
            AssignedAvatarUrl = assignedAvatarUrl;
        }
    }

    public class KanbanSubtaskDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public Priority Priority { get; set; } = Priority.Low;
        public int Order { get; set; }

        public KanbanSubtaskDto() { }
        public KanbanSubtaskDto(Guid id, Guid taskId, string title, bool isCompleted, Priority priority, int order)
        {
            Id = id;
            TaskId = taskId;
            Title = title;
            IsCompleted = isCompleted;
            Priority = priority;
            Order = order;
        }
    }

    public record CreateBoardRequest(string Name, string? Description);
    public record CreateColumnRequest(string Name, int Order);
    public record CreateTaskRequest(string Title, string? Description, Priority Priority, DateTime? DueDate, string? Color, int Order);
    public record CreateSubtaskRequest(string Title, Priority Priority, int Order);

    public record UpdateTaskRequest(
        string Title, 
        string? Description, 
        Priority Priority, 
        DateTime? DueDate, 
        string? Color, 
        bool IsArchived,
        Guid? AssignedUserId);

    public record ReorderColumnRequest(Guid ColumnId, int NewOrder);
    public record MoveTaskRequest(Guid TaskId, Guid NewColumnId, int NewOrder);
}
