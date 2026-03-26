using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flux.Features.Kanban
{
    [Authorize]
    [ApiController]
    [Route("api/workspaces/{workspaceId:guid}/kanban")]
    public class KanbanController : ControllerBase
    {
        private readonly FluxDbContext _db;

        public KanbanController(FluxDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<KanbanBoardDto>>> GetBoards(Guid workspaceId)
        {
            var boards = await _db.KanbanBoards
                .Where(b => b.WorkspaceId == workspaceId)
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.Subtasks)
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                        .ThenInclude(t => t.AssignedUser)
                .OrderBy(b => b.CreatedAt)
                .ToListAsync();

            return boards.Select(b => new KanbanBoardDto(
                b.Id,
                b.WorkspaceId,
                b.Name,
                b.Description,
                b.Columns.OrderBy(c => c.Order).Select(c => new KanbanColumnDto(
                    c.Id,
                    c.Name,
                    c.Order,
                    c.Tasks.OrderBy(t => t.Order).Select(t => new KanbanTaskDto(
                        t.Id,
                        t.ColumnId,
                        t.Title,
                        t.Description,
                        t.Priority,
                        t.DueDate,
                        t.Color,
                        t.Order,
                        t.IsArchived,
                        t.Subtasks.OrderBy(s => s.Order).Select(s => new KanbanSubtaskDto(
                            s.Id, s.TaskId, s.Title, s.IsCompleted, s.Priority, s.Order
                        )).ToList(),
                        t.AssignedUserId,
                        t.AssignedUser?.Username,
                        t.AssignedUser?.AvatarUrl
                    )).ToList()
                )).ToList()
            )).ToList();
        }

        [HttpPost("boards")]
        public async Task<ActionResult<KanbanBoardDto>> CreateBoard(Guid workspaceId, CreateBoardRequest request)
        {
            var board = new KanbanBoard
            {
                WorkspaceId = workspaceId,
                Name = request.Name,
                Description = request.Description
            };

            _db.KanbanBoards.Add(board);
            await _db.SaveChangesAsync();

            return Ok(new KanbanBoardDto(board.Id, board.WorkspaceId, board.Name, board.Description, new List<KanbanColumnDto>()));
        }

        [HttpPost("boards/{boardId:guid}/columns")]
        public async Task<ActionResult<KanbanColumnDto>> CreateColumn(Guid boardId, CreateColumnRequest request)
        {
            var column = new KanbanColumn
            {
                BoardId = boardId,
                Name = request.Name,
                Order = request.Order
            };

            _db.KanbanColumns.Add(column);
            await _db.SaveChangesAsync();

            return Ok(new KanbanColumnDto(column.Id, column.Name, column.Order, new List<KanbanTaskDto>()));
        }

        [HttpPost("columns/{columnId:guid}/tasks")]
        public async Task<ActionResult<KanbanTaskDto>> CreateTask(Guid columnId, CreateTaskRequest request)
        {
            var task = new KanbanTask
            {
                ColumnId = columnId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                DueDate = request.DueDate,
                Color = request.Color,
                Order = request.Order
            };

            _db.KanbanTasks.Add(task);
            await _db.SaveChangesAsync();

            return Ok(new KanbanTaskDto(
                task.Id, task.ColumnId, task.Title, task.Description, task.Priority,
                task.DueDate, task.Color, task.Order, task.IsArchived, 
                new List<KanbanSubtaskDto>(), null, null, null));
        }

        [HttpPatch("tasks/{taskId:guid}")]
        public async Task<IActionResult> UpdateTask(Guid taskId, UpdateTaskRequest request)
        {
            var task = await _db.KanbanTasks.FindAsync(taskId);
            if (task == null) return NotFound();

            task.Title = request.Title;
            task.Description = request.Description;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.Color = request.Color;
            task.IsArchived = request.IsArchived;
            task.AssignedUserId = request.AssignedUserId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("tasks/{taskId:guid}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var task = await _db.KanbanTasks.FindAsync(taskId);
            if (task == null) return NotFound();

            _db.KanbanTasks.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("tasks/{taskId:guid}/subtasks")]
        public async Task<ActionResult<KanbanSubtaskDto>> CreateSubtask(Guid taskId, CreateSubtaskRequest request)
        {
            var subtask = new KanbanSubtask
            {
                TaskId = taskId,
                Title = request.Title,
                Priority = request.Priority,
                Order = request.Order
            };

            _db.KanbanSubtasks.Add(subtask);
            await _db.SaveChangesAsync();

            return Ok(new KanbanSubtaskDto(subtask.Id, subtask.TaskId, subtask.Title, subtask.IsCompleted, subtask.Priority, subtask.Order));
        }

        [HttpPatch("subtasks/{subtaskId:guid}/toggle")]
        public async Task<IActionResult> ToggleSubtask(Guid subtaskId)
        {
            var subtask = await _db.KanbanSubtasks.FindAsync(subtaskId);
            if (subtask == null) return NotFound();

            subtask.IsCompleted = !subtask.IsCompleted;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("subtasks/{subtaskId:guid}")]
        public async Task<IActionResult> DeleteSubtask(Guid subtaskId)
        {
            var subtask = await _db.KanbanSubtasks.FindAsync(subtaskId);
            if (subtask == null) return NotFound();

            _db.KanbanSubtasks.Remove(subtask);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("reorder-columns")]
        public async Task<IActionResult> ReorderColumns(List<ReorderColumnRequest> requests)
        {
            foreach (var req in requests)
            {
                var col = await _db.KanbanColumns.FindAsync(req.ColumnId);
                if (col != null) col.Order = req.NewOrder;
            }
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("move-task")]
        public async Task<IActionResult> MoveTask(MoveTaskRequest request)
        {
            var task = await _db.KanbanTasks.FindAsync(request.TaskId);
            if (task == null) return NotFound();

            task.ColumnId = request.NewColumnId;
            task.Order = request.NewOrder;

            // Optional: Reorder other tasks in the same column? 
            // Simple approach for now: just update this one.
            
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
