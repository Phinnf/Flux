using Flux.Domain.Common;
using Flux.Features.Kanban;

namespace Flux.Infrastructure.Client;

public class KanbanStateService
{
    private readonly KanbanClientService _kanbanService;

    public KanbanStateService(KanbanClientService kanbanService)
    {
        _kanbanService = kanbanService;
    }

    public List<KanbanBoardDto> Boards { get; private set; } = new();
    public KanbanBoardDto? SelectedBoard { get; private set; }

    public event Action? OnStateChanged;

    public async Task LoadBoardsAsync(Guid workspaceId)
    {
        var result = await _kanbanService.GetBoardsAsync(workspaceId);
        if (result.IsSuccess)
        {
            Boards = result.Value ?? new();
            if (Boards.Count == 0)
            {
                // Create a sample board for new workspaces/channels
                var sampleResult = await CreateBoardAsync(workspaceId, "Project Board", "Default board for your workflow");
                if (sampleResult.IsSuccess)
                {
                    var board = sampleResult.Value!;
                    await CreateColumnAsync(workspaceId, board.Id, "To Do");
                    await CreateColumnAsync(workspaceId, board.Id, "In Progress");
                    await CreateColumnAsync(workspaceId, board.Id, "Done");
                    
                    // Reload to get columns
                    var refreshed = await _kanbanService.GetBoardsAsync(workspaceId);
                    if (refreshed.IsSuccess)
                    {
                        Boards = refreshed.Value ?? new();
                    }
                }
            }

            if (SelectedBoard == null && Boards.Count > 0)
            {
                SelectedBoard = Boards[0];
            }
            else if (SelectedBoard != null)
            {
                // Try to preserve selection if it still exists in the new list
                SelectedBoard = Boards.FirstOrDefault(b => b.Id == SelectedBoard.Id) ?? (Boards.Count > 0 ? Boards[0] : null);
            }
        }
        NotifyStateChanged();
    }

    public void SelectBoard(Guid boardId)
    {
        SelectedBoard = Boards.FirstOrDefault(b => b.Id == boardId);
        NotifyStateChanged();
    }

    public async Task<Result<KanbanBoardDto>> CreateBoardAsync(Guid workspaceId, string name, string? description)
    {
        var result = await _kanbanService.CreateBoardAsync(workspaceId, new CreateBoardRequest(name, description));
        if (result.IsSuccess)
        {
            Boards.Add(result.Value!);
            SelectedBoard = result.Value;
            NotifyStateChanged();
        }
        return result;
    }

    public async Task<Result<KanbanColumnDto>> CreateColumnAsync(Guid workspaceId, Guid boardId, string name)
    {
        int nextOrder = (SelectedBoard?.Columns.Count ?? 0);
        var result = await _kanbanService.CreateColumnAsync(workspaceId, boardId, new CreateColumnRequest(name, nextOrder));
        if (result.IsSuccess && SelectedBoard != null && SelectedBoard.Id == boardId)
        {
            SelectedBoard.Columns.Add(result.Value!);
            NotifyStateChanged();
        }
        return result;
    }

    public async Task<Result<KanbanTaskDto>> CreateTaskAsync(Guid workspaceId, Guid columnId, string title)
    {
        var column = SelectedBoard?.Columns.FirstOrDefault(c => c.Id == columnId);
        int nextOrder = (column?.Tasks.Count ?? 0);
        
        var result = await _kanbanService.CreateTaskAsync(workspaceId, columnId, new CreateTaskRequest(title, null, Domain.Entities.Priority.Low, null, null, nextOrder));
        if (result.IsSuccess && column != null)
        {
            column.Tasks.Add(result.Value!);
            NotifyStateChanged();
        }
        return result;
    }

    public async Task MoveTaskAsync(Guid workspaceId, Guid taskId, Guid sourceColumnId, Guid targetColumnId, int newOrder)
    {
        if (SelectedBoard == null) return;

        var sourceColumn = SelectedBoard.Columns.FirstOrDefault(c => c.Id == sourceColumnId);
        var targetColumn = SelectedBoard.Columns.FirstOrDefault(c => c.Id == targetColumnId);
        var task = sourceColumn?.Tasks.FirstOrDefault(t => t.Id == taskId);

        if (task == null || targetColumn == null) return;

        // Local update for immediate feedback
        sourceColumn!.Tasks.Remove(task);
        task.ColumnId = targetColumnId;
        targetColumn.Tasks.Insert(Math.Min(newOrder, targetColumn.Tasks.Count), task);
        
        NotifyStateChanged();

        // Server update
        await _kanbanService.MoveTaskAsync(workspaceId, new MoveTaskRequest(taskId, targetColumnId, newOrder));
    }

    public async Task<Result> UpdateTaskAsync(Guid workspaceId, Guid taskId, UpdateTaskRequest request)
    {
        var result = await _kanbanService.UpdateTaskAsync(workspaceId, taskId, request);
        if (result.IsSuccess) NotifyStateChanged();
        return result;
    }

    public async Task<Result<KanbanSubtaskDto>> CreateSubtaskAsync(Guid workspaceId, Guid taskId, CreateSubtaskRequest request)
    {
        var result = await _kanbanService.CreateSubtaskAsync(workspaceId, taskId, request);
        if (result.IsSuccess) NotifyStateChanged();
        return result;
    }

    public async Task<Result> ToggleSubtaskAsync(Guid workspaceId, Guid subtaskId)
    {
        var result = await _kanbanService.ToggleSubtaskAsync(workspaceId, subtaskId);
        if (result.IsSuccess) NotifyStateChanged();
        return result;
    }

    public async Task<Result> DeleteSubtaskAsync(Guid workspaceId, Guid subtaskId)
    {
        var result = await _kanbanService.DeleteSubtaskAsync(workspaceId, subtaskId);
        if (result.IsSuccess) NotifyStateChanged();
        return result;
    }

    public async Task<Result> DeleteTaskAsync(Guid workspaceId, Guid taskId)
    {
        var result = await _kanbanService.DeleteTaskAsync(workspaceId, taskId);
        if (result.IsSuccess)
        {
            // Local cleanup
            foreach (var col in SelectedBoard?.Columns ?? new())
            {
                var task = col.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    col.Tasks.Remove(task);
                    break;
                }
            }
            NotifyStateChanged();
        }
        return result;
    }

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}
