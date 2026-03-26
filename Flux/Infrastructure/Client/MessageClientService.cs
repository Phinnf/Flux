using System.Net.Http.Json;
using Flux.Domain.Common;
using Flux.Features.Messages.GetMessages;
using Flux.Features.Messages.SendMessage;
using Flux.Features.Messages.EditMessage;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class MessageClientService : BaseClientService
{
    public MessageClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<List<MessageDto>>> GetMessagesAsync(Guid channelId, DateTime? before = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = $"/api/channels/{channelId}/messages";
            if (before.HasValue)
            {
                url += $"?before={Uri.EscapeDataString(before.Value.ToString("O"))}";
            }
            
            var response = await HttpClient.GetFromJsonAsync<Result<List<MessageDto>>>(url);
            return response ?? Result<List<MessageDto>>.CreateFailure("Failed to load messages.");
        }
        catch (Exception ex)
        {
            return Result<List<MessageDto>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<List<MessageDto>>> GetThreadsAsync(Guid workspaceId, Guid userId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<Result<List<MessageDto>>>($"/api/workspaces/{workspaceId}/threads?userId={userId}");
            return response ?? Result<List<MessageDto>>.CreateFailure("Failed to load threads.");
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync("/api/messages", command);
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.PutAsJsonAsync($"/api/messages/{messageId}", request);
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.DeleteAsync($"/api/messages/{messageId}?userId={userId}");
            var result = await response.Content.ReadFromJsonAsync<Result>();
            return result ?? Result.Failure("Failed to delete message.");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> ToggleReactionAsync(Guid messageId, Guid userId, string emoji)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { UserId = userId, Emoji = emoji };
            var response = await HttpClient.PostAsJsonAsync($"/api/messages/{messageId}/reactions", request);
            
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
