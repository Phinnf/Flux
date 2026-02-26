using System.Net.Http.Json;
using Flux.Domain.Entities;

namespace Flux.Infrastructure.Client;

/// <summary>
/// Simplified record for Workspace in UI.
/// </summary>
public record WorkspaceSummary(Guid Id, string Name, string? Description, DateTime CreatedAt);

/// <summary>
/// Simplified record for Channel in UI.
/// </summary>
public record ChannelSummary(Guid Id, string Name, string? Description, ChannelType Type);

/// <summary>
/// Simplified record for Message in UI.
/// </summary>
public record MessageSummary(Guid Id, string Content, Guid UserId, string Username, DateTime CreatedAt);

/// <summary>
/// Request DTO for sending a message.
/// </summary>
public record SendMessageRequest(string Content, Guid ChannelId, Guid UserId);

/// <summary>
/// Service to communicate with our API endpoints.
/// </summary>
public class FluxClientService
{
    private readonly HttpClient _httpClient;

    public FluxClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets all workspaces for a given user.
    /// </summary>
    public async Task<List<WorkspaceSummary>> GetWorkspacesAsync(Guid userId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<WorkspaceSummary>>($"/api/workspaces?userId={userId}");
        return response ?? new List<WorkspaceSummary>();
    }

    /// <summary>
    /// Gets all channels for a workspace that the user has access to.
    /// </summary>
    public async Task<List<ChannelSummary>> GetChannelsAsync(Guid workspaceId, Guid userId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<ChannelSummary>>($"/api/workspaces/{workspaceId}/channels?userId={userId}");
        return response ?? new List<ChannelSummary>();
    }

    /// <summary>
    /// Gets messages for a specific channel.
    /// </summary>
    public async Task<List<MessageSummary>> GetMessagesAsync(Guid channelId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<MessageSummary>>($"/api/channels/{channelId}/messages");
        return response ?? new List<MessageSummary>();
    }

    /// <summary>
    /// Sends a message via the API.
    /// </summary>
    public async Task SendMessageAsync(SendMessageRequest request)
    {
        await _httpClient.PostAsJsonAsync("/api/messages", request);
    }
}
