using System.Net.Http.Json;
using Flux.Domain.Common;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class UserClientService : BaseClientService
{
    public UserClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.GetAsync($"/api/users/profile?userId={userId}");
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

    public async Task<Result> UpdateProfileAsync(Guid userId, string? username, string? fullName, string? nickName, string? gender, string? country, string? avatarUrl, string? status, string? newPassword)
    {
        try
        {
            await SetAuthHeaderAsync();
            var request = new { 
                Username = username,
                FullName = fullName,
                NickName = nickName,
                Gender = gender,
                Country = country,
                AvatarUrl = avatarUrl,
                Status = status,
                NewPassword = newPassword
            };
            var response = await HttpClient.PutAsJsonAsync($"/api/users/{userId}/profile", request);
            
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

    public async Task<Result> DeleteAccountAsync(Guid userId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await HttpClient.DeleteAsync($"/api/users/{userId}");
            
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
