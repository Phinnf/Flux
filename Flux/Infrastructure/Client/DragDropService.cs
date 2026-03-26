using Flux.Features.Kanban;

namespace Flux.Infrastructure.Client;

public static class DragDropService
{
    public static KanbanTaskDto? DraggedTask { get; set; }
}
