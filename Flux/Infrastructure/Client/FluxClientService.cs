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
    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await httpClient.PostAsJsonAsync("/api/users/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Token != null ? Result<string>.CreateSuccess(result.Token) : Result<string>.CreateFailure("Invalid token received.");
            }
            
            return Result<string>.CreateFailure("Invalid email or password.");
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result<string>> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var request = new { Username = username, Email = email, Password = password };
            var response = await httpClient.PostAsJsonAsync("/api/users/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                // Note: The backend currently returns string token directly if successful, but usually it returns a JSON object.
                // Depending on the exact RegisterController implementation, let's assume it returns a raw string or we can read it.
                var content = await response.Content.ReadAsStringAsync();
                return Result<string>.CreateSuccess(content); 
            }
            
            return Result<string>.CreateFailure("Registration failed.");
        }
        catch (Exception ex)
        {
            return Result<string>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> VerifyEmailAsync(string email, string otp)
    {
        try
        {
            var request = new { Email = email, Otp = otp };
            var response = await httpClient.PostAsJsonAsync("/api/users/verify-email", request);
            
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

    private class LoginResponse { public string? Token { get; set; } }

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

    public async Task<Result> RenameChannelAsync(Guid workspaceId, Guid channelId, string newName, Guid userId)
    {
        try
        {
            var request = new { NewName = newName, UserId = userId };
            var response = await httpClient.PutAsJsonAsync($"/api/workspaces/{workspaceId}/channels/{channelId}/rename", request);
            
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

    public async Task<Result<string>> CreateInviteAsync(Guid workspaceId, Guid userId, int? expiresInHours)
    {
        try
        {
            var request = new { ExpiresInHours = expiresInHours, UserId = userId };
            var response = await httpClient.PostAsJsonAsync($"/api/workspaces/{workspaceId}/invites", request);
            
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

    public record InviteDetailsDto(Guid WorkspaceId, string WorkspaceName, string? WorkspaceDescription);

    public async Task<Result<InviteDetailsDto>> GetInviteDetailsAsync(string code)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/invites/{code}");
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
            var request = new { UserId = userId };
            var response = await httpClient.PostAsJsonAsync($"/api/invites/{code}/join", request);
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

    public record UserProfileDto(Guid Id, string Username, string Email, string? FullName, string? NickName, string? Gender, string? Country, string? AvatarUrl);

    public async Task<Result<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/users/profile?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                return result != null 
                    ? Result<UserProfileDto>.CreateSuccess(result) 
                    : Result<UserProfileDto>.CreateFailure("Failed to deserialize profile.");
            }
            
            var error = await response.Content.ReadAsStringAsync();
            return Result<UserProfileDto>.CreateFailure(error);
        }
        catch (Exception ex)
        {
            return Result<UserProfileDto>.CreateFailure(ex.Message);
        }
    }

    public async Task<Result> UpdateProfileAsync(Guid userId, string? username, string? fullName, string? nickName, string? gender, string? country, string? avatarUrl)
    {
        try
        {
            var request = new { UserId = userId, Username = username, FullName = fullName, NickName = nickName, Gender = gender, Country = country, AvatarUrl = avatarUrl };
            var response = await httpClient.PutAsJsonAsync("/api/users/profile", request);
            
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

    public async Task<Result> SendOtpAsync(Guid userId)
    {
        try
        {
            var request = new { UserId = userId };
            var response = await httpClient.PostAsJsonAsync("/api/users/profile/send-otp", request);
            
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

    public async Task<Result> ChangePasswordAsync(Guid userId, string otp, string newPassword)
    {
        try
        {
            var request = new { UserId = userId, Otp = otp, NewPassword = newPassword };
            var response = await httpClient.PostAsJsonAsync("/api/users/profile/change-password", request);
            
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
