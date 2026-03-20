import os
import re

client_dir = 'Flux/Infrastructure/Client'
file_path = os.path.join(client_dir, 'FluxClientService.cs')

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Let's extract the DTOs first
dtos_code = '''using System;

namespace Flux.Infrastructure.Client;

public record WorkspaceSummary(Guid Id, string Name, string? Description, DateTime CreatedAt);
public record ChannelSummary(Guid Id, string Name, string? Description, Flux.Domain.Entities.ChannelType Type);
public record MemberDto(Guid Id, string Username, string? FullName, string? AvatarUrl, string? Status);
public record InviteDetailsDto(Guid WorkspaceId, string WorkspaceName, string? WorkspaceDescription);
public record UserProfileDto(Guid Id, string Username, string Email, string? FullName, string? NickName, string? Gender, string? Country, string? AvatarUrl, string? Status);

public class LoginResponse { public string? Token { get; set; } }
public class RegisterResponse { public string? Message { get; set; } }
public class ErrorResponse { public string? Error { get; set; } }
'''

with open(os.path.join(client_dir, 'ClientDtos.cs'), 'w', encoding='utf-8') as f:
    f.write(dtos_code)

# Base Client Service
base_service_code = '''using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Flux.Infrastructure.Client;

public abstract class BaseClientService
{
    protected readonly HttpClient HttpClient;
    protected readonly IJSRuntime JsRuntime;

    protected BaseClientService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        HttpClient = httpClient;
        JsRuntime = jsRuntime;
    }

    protected async Task SetAuthHeaderAsync()
    {
        try
        {
            var token = await JsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await JsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Ignore JS interop errors
        }
    }
}
'''

with open(os.path.join(client_dir, 'BaseClientService.cs'), 'w', encoding='utf-8') as f:
    f.write(base_service_code)

print('Generated DTOs and Base service')
