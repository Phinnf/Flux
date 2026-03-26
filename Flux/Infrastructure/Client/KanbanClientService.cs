using System.Net.Http.Json;
using Flux.Domain.Common;
using Flux.Features.Kanban;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class KanbanClientService : BaseClientService
{
    public KanbanClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<List<KanbanBoardDto>>> GetBoardsAsync(Guid workspaceId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<List<KanbanBoardDto>>($"/api/workspaces/{workspaceId}/kanban");
            return response != null ? Result<List<KanbanBoardDto>>.CreateSuccess(response) : Result<List<KanbanBoardDto>>.CreateFailure("Failed to load boards.");
        }
        catch (Exception ex)
        {
            return Result<List<KanbanBoardDto>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<KanbanBoardDto>> CreateBoardAsync(Guid workspaceId, CreateBoardRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/boards", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<KanbanBoardDto>();
                return result != null ? Result<KanbanBoardDto>.CreateSuccess(result) : Result<KanbanBoardDto>.CreateFailure("Failed to deserialize board.");
            }
            return Result<KanbanBoardDto>.CreateFailure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result<KanbanBoardDto>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<KanbanColumnDto>> CreateColumnAsync(Guid workspaceId, Guid boardId, CreateColumnRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/boards/{boardId}/columns", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<KanbanColumnDto>();
                return result != null ? Result<KanbanColumnDto>.CreateSuccess(result) : Result<KanbanColumnDto>.CreateFailure("Failed to deserialize column.");
            }
            return Result<KanbanColumnDto>.CreateFailure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result<KanbanColumnDto>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<KanbanTaskDto>> CreateTaskAsync(Guid workspaceId, Guid columnId, CreateTaskRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/columns/{columnId}/tasks", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<KanbanTaskDto>();
                return result != null ? Result<KanbanTaskDto>.CreateSuccess(result) : Result<KanbanTaskDto>.CreateFailure("Failed to deserialize task.");
            }
            return Result<KanbanTaskDto>.CreateFailure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result<KanbanTaskDto>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> UpdateTaskAsync(Guid workspaceId, Guid taskId, UpdateTaskRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PatchAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/tasks/{taskId}", request);
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteTaskAsync(Guid workspaceId, Guid taskId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.DeleteAsync($"/api/workspaces/{workspaceId}/kanban/tasks/{taskId}");
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<KanbanSubtaskDto>> CreateSubtaskAsync(Guid workspaceId, Guid taskId, CreateSubtaskRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/tasks/{taskId}/subtasks", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<KanbanSubtaskDto>();
                return result != null ? Result<KanbanSubtaskDto>.CreateSuccess(result) : Result<KanbanSubtaskDto>.CreateFailure("Failed to deserialize subtask.");
            }
            return Result<KanbanSubtaskDto>.CreateFailure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result<KanbanSubtaskDto>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> ToggleSubtaskAsync(Guid workspaceId, Guid subtaskId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PatchAsync($"/api/workspaces/{workspaceId}/kanban/subtasks/{subtaskId}/toggle", null);
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteSubtaskAsync(Guid workspaceId, Guid subtaskId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.DeleteAsync($"/api/workspaces/{workspaceId}/kanban/subtasks/{subtaskId}");
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> ReorderColumnsAsync(Guid workspaceId, List<ReorderColumnRequest> requests)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/reorder-columns", requests);
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> MoveTaskAsync(Guid workspaceId, MoveTaskRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/kanban/move-task", request);
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
