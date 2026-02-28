using System.Net.Http.Json;
using Flux.Domain.Common;
using Flux.Features.Messages.GetMessages;
using Flux.Features.Messages.SendMessage;
using Flux.Features.Messages.EditMessage;
using Flux.Features.Channels.CreateChannel;
using Flux.Domain.Entities;

namespace Flux.Infrastructure.Client;

public record WorkspaceSummary(Guid Id, string Name, string? Description, DateTime CreatedAt);

public record ChannelSummary(Guid Id, string Name, string? Description, ChannelType Type);

public class FluxClientService(HttpClient httpClient)
{
    public async Task<Result<WorkspaceSummary>> CreateWorkspaceAsync(Guid userId, string name, string? description)
    {
        try
        {
            var request = new { Name = name, Description = description };
            var response = await httpClient.PostAsJsonAsync($"/api/workspaces?userId={userId}", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<WorkspaceSummary>>();
                return result ?? Result<WorkspaceSummary>.CreateFailure("Failed to deserialize workspace.");
            }
            
            return Result<WorkspaceSummary>.CreateFailure("Failed to create workspace.");
        }
        catch (Exception ex)
        {
            return Result<WorkspaceSummary>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<List<WorkspaceSummary>>> GetWorkspacesAsync(Guid userId)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<Result<List<WorkspaceSummary>>>($"/api/workspaces?userId={userId}");
            return response ?? Result<List<WorkspaceSummary>>.CreateFailure("Failed to load workspaces.");
        }
        catch (Exception ex)
        {
            return Result<List<WorkspaceSummary>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> DeleteWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/api/workspaces/{workspaceId}?userId={userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Failure("Failed to deserialize response.");
            }
            
            return Result.Failure("Failed to delete workspace.");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<List<ChannelSummary>>> GetChannelsAsync(Guid workspaceId, Guid userId)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<Result<List<ChannelSummary>>>($"/api/workspaces/{workspaceId}/channels?userId={userId}");
            return response ?? Result<List<ChannelSummary>>.CreateFailure("Failed to load channels.");
        }
        catch (Exception ex)
        {
            return Result<List<ChannelSummary>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<List<MessageDto>>> GetMessagesAsync(Guid channelId, DateTime? before = null)
    {
        try
        {
            var url = $"/api/channels/{channelId}/messages";
            if (before.HasValue)
            {
                url += $"?before={Uri.EscapeDataString(before.Value.ToString("O"))}";
            }
            
            var response = await httpClient.GetFromJsonAsync<Result<List<MessageDto>>>(url);
            return response ?? Result<List<MessageDto>>.CreateFailure("Failed to load messages.");
        }
        catch (Exception ex)
        {
            return Result<List<MessageDto>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<SendMessageResponse>> SendMessageAsync(SendMessageCommand command)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/messages", command);
            var result = await response.Content.ReadFromJsonAsync<Result<SendMessageResponse>>();
            return result ?? Result<SendMessageResponse>.CreateFailure("Failed to send message.");
        }
        catch (Exception ex)
        {
            return Result<SendMessageResponse>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<EditMessageResponse>> EditMessageAsync(Guid messageId, EditMessageRequest request)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"/api/messages/{messageId}", request);
            var result = await response.Content.ReadFromJsonAsync<Result<EditMessageResponse>>();
            return result ?? Result<EditMessageResponse>.CreateFailure("Failed to edit message.");
        }
        catch (Exception ex)
        {
            return Result<EditMessageResponse>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> DeleteMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/api/messages/{messageId}?userId={userId}");
            var result = await response.Content.ReadFromJsonAsync<Result>();
            return result ?? Result.Failure("Failed to delete message.");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<CreateChannelResponse>> CreateChannelAsync(Guid workspaceId, CreateChannelRequest request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/channels", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateChannelResponse>();
                return result != null ? Result<CreateChannelResponse>.CreateSuccess(result) : Result<CreateChannelResponse>.CreateFailure("Failed to deserialize channel.");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result<CreateChannelResponse>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<CreateChannelResponse>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> DeleteChannelAsync(Guid workspaceId, Guid channelId, Guid userId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/api/workspaces/{workspaceId}/channels/{channelId}?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result.Failure(error);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
