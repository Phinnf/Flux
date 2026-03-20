import os

client_dir = 'Flux/Infrastructure/Client'

auth_service_code = '''using System.Net.Http.Json;
using Flux.Domain.Common;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public class AuthClientService : BaseClientService
{
    public AuthClientService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient, jsRuntime) { }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await HttpClient.PostAsJsonAsync("/api/users/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Token != null ? Result<string>.CreateSuccess(result.Token) : Result<string>.CreateFailure("Invalid token received.");
            }
            
            var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return Result<string>.CreateFailure(errorResult?.Error ?? "Invalid email or password.");
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
            var response = await HttpClient.PostAsJsonAsync("/api/users/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                return Result<string>.CreateSuccess(result?.Message ?? "Registration successful."); 
            }
            
            var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return Result<string>.CreateFailure(errorResult?.Error ?? "Registration failed.");
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
            var response = await HttpClient.PostAsJsonAsync("/api/users/verify-email", request);
            
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }
            
            var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return Result.Failure(errorResult?.Error ?? "Verification failed.");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
'''

with open(os.path.join(client_dir, 'AuthClientService.cs'), 'w', encoding='utf-8') as f:
    f.write(auth_service_code)

user_service_code = '''using System.Net.Http.Json;
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
}
'''

with open(os.path.join(client_dir, 'UserClientService.cs'), 'w', encoding='utf-8') as f:
    f.write(user_service_code)

print('Generated Auth and User services')
