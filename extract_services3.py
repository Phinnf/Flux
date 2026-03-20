import os

client_dir = 'Flux/Infrastructure/Client'

workspace_service_code = '''using System.Net.Http.Json;
using Flux.Domain.Common;
using Flux.Features.Channels.CreateChannel;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class WorkspaceClientService : BaseClientService
{
    public WorkspaceClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<List<MemberDto>>> GetWorkspaceMembersAsync(Guid workspaceId, Guid userId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<List<MemberDto>>($"/api/workspaces/{workspaceId}/members?userId={userId}");
            return response != null ? Result<List<MemberDto>>.CreateSuccess(response) : Result<List<MemberDto>>.CreateFailure("Failed to load members.");
        }
        catch (Exception ex)
        {
            return Result<List<MemberDto>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<WorkspaceSummary>> CreateWorkspaceAsync(Guid userId, string name, string? description)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { Name = name, Description = description };
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces?userId={userId}", request);
            
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<Result<List<WorkspaceSummary>>>($"/api/workspaces?userId={userId}");
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.DeleteAsync($"/api/workspaces/{workspaceId}?userId={userId}");
            
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetFromJsonAsync<Result<List<ChannelSummary>>>($"/api/workspaces/{workspaceId}/channels?userId={userId}");
            return response ?? Result<List<ChannelSummary>>.CreateFailure("Failed to load channels.");
        }
        catch (Exception ex)
        {
            return Result<List<ChannelSummary>>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<CreateChannelResponse>> CreateChannelAsync(Guid workspaceId, CreateChannelRequest request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/channels", request);
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
            await SetAuthHeaderAsync();
            var response = await HttpClient.DeleteAsync($"/api/workspaces/{workspaceId}/channels/{channelId}?userId={userId}");
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

    public async Task<Result> RenameChannelAsync(Guid workspaceId, Guid channelId, string newName, Guid userId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { NewName = newName, UserId = userId };
            var response = await HttpClient.PutAsJsonAsync($"/api/workspaces/{workspaceId}/channels/{channelId}/rename", request);
            
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

    public async Task<Result<ChannelSummary>> GetOrCreateDirectChannelAsync(Guid workspaceId, Guid currentUserId, Guid targetUserId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { TargetUserId = targetUserId, CurrentUserId = currentUserId };
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/channels/direct", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ChannelSummary>();
                return result != null ? Result<ChannelSummary>.CreateSuccess(result) : Result<ChannelSummary>.CreateFailure("Failed to load direct channel.");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result<ChannelSummary>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<ChannelSummary>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<string>> CreateInviteAsync(Guid workspaceId, Guid userId, int? expiresInHours)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { ExpiresInHours = expiresInHours, UserId = userId };
            var response = await HttpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/invites", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return result != null && result.TryGetValue("code", out var code) 
                    ? Result<string>.CreateSuccess(code) 
                    : Result<string>.CreateFailure("Invalid response format.");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result<string>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<InviteDetailsDto>> GetInviteDetailsAsync(string code)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetAsync($"/api/invites/{code}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<InviteDetailsDto>();
                return result != null 
                    ? Result<InviteDetailsDto>.CreateSuccess(result) 
                    : Result<InviteDetailsDto>.CreateFailure("Failed to deserialize invite details.");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result<InviteDetailsDto>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<InviteDetailsDto>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<Guid>> JoinWorkspaceViaInviteAsync(string code, Guid userId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { UserId = userId };
            var response = await HttpClient.PostAsJsonAsync($"/api/invites/{code}/join", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
                return result != null && result.TryGetValue("workspaceId", out var wsId) 
                    ? Result<Guid>.CreateSuccess(wsId) 
                    : Result<Guid>.CreateFailure("Invalid response format.");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result<Guid>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<Guid>.CreateFailure(ex.Message);
        }
    }
}
'''

with open(os.path.join(client_dir, 'WorkspaceClientService.cs'), 'w', encoding='utf-8') as f:
    f.write(workspace_service_code)

message_service_code = '''using System.Net.Http.Json;
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
'''

with open(os.path.join(client_dir, 'MessageClientService.cs'), 'w', encoding='utf-8') as f:
    f.write(message_service_code)

upload_service_code = '''using System.Net.Http.Json;
using System.Text.Json;
using Flux.Domain.Common;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class UploadClientService : BaseClientService
{
    public UploadClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<string>> UploadImageAsync(MultipartFormDataContent content)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsync("/api/uploads/image", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Result<string>.CreateSuccess(result.GetProperty("url").GetString()!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return Result<string>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<string>> UploadAudioAsync(MultipartFormDataContent content)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.PostAsync("/api/uploads/audio", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Result<string>.CreateSuccess(result.GetProperty("url").GetString()!);
            }
            var error = await response.Content.ReadAsStringAsync();
            return Result<string>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }
}
'''

with open(os.path.join(client_dir, 'UploadClientService.cs'), 'w', encoding='utf-8') as f:
    f.write(upload_service_code)

print('Generated Workspace, Message, and Upload services')
