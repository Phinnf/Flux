using System;

namespace Flux.Infrastructure.Client;

public record WorkspaceSummary(Guid Id, string Name, string? Description, DateTime CreatedAt);
public record ChannelSummary(Guid Id, string Name, string? Description, Flux.Domain.Entities.ChannelType Type);
public record MemberDto(Guid Id, string Username, string? FullName, string? AvatarUrl, string? Status);
public record InviteDetailsDto(Guid WorkspaceId, string WorkspaceName, string? WorkspaceDescription);
public record UserProfileDto(Guid Id, string Username, string Email, string? FullName, string? NickName, string? Gender, string? Country, string? AvatarUrl, string? Status);

public class LoginResponse { public string? Token { get; set; } public bool RequiresTwoFactor { get; set; } }
public class RegisterResponse { public string? Message { get; set; } }
public class ErrorResponse { public string? Error { get; set; } }
