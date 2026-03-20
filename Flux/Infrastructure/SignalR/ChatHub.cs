using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Flux.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Flux.Infrastructure.SignalR;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<string, int> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _callParticipants = new();
    private static readonly Dictionary<string, string> _connectionToCallMap = new();
    private readonly IServiceProvider _serviceProvider;

    public ChatHub(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            bool isFirstConnection = false;
            lock (_userConnections)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = 0;
                    isFirstConnection = true;
                }
                _userConnections[userId]++;
            }

            if (isFirstConnection)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FluxDbContext>();
                if (Guid.TryParse(userId, out var uid))
                {
                    var user = await dbContext.Users.FindAsync(uid);
                    if (user != null)
                    {
                        // Preserve custom statuses like "Idle" or "Working" if they reconnect.
                        // Only change to "Online" if they were previously "Offline".
                        if (user.Status == "Offline" || string.IsNullOrEmpty(user.Status))
                        {
                            user.Status = "Online";
                            await dbContext.SaveChangesAsync();
                        }
                        
                        await Clients.All.SendAsync("UserPresenceChanged", userId, user.Status);
                    }
                }
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            bool isLastConnection = false;
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId]--;
                    if (_userConnections[userId] <= 0)
                    {
                        _userConnections.Remove(userId);
                        isLastConnection = true;
                    }
                }
            }

            if (isLastConnection)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FluxDbContext>();
                if (Guid.TryParse(userId, out var uid))
                {
                    var user = await dbContext.Users.FindAsync(uid);
                    if (user != null)
                    {
                        user.Status = "Offline";
                        await dbContext.SaveChangesAsync();
                    }
                }

                await Clients.All.SendAsync("UserPresenceChanged", userId, "Offline");
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UpdateStatus(string status)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FluxDbContext>();
            if (Guid.TryParse(userId, out var uid))
            {
                var user = await dbContext.Users.FindAsync(uid);
                if (user != null)
                {
                    user.Status = status;
                    await dbContext.SaveChangesAsync();
                }
            }
            await Clients.All.SendAsync("UserPresenceChanged", userId, status);
        }
    }

    // Users will join a group named after the ChannelId
    public async Task JoinChannel(string channelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
    }

    public async Task LeaveChannel(string channelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
    }

    // --- WebRTC Signaling ---
    
    public async Task JoinCall(string callId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"call_{callId}");
        // Notify others in the call that a new user joined
        await Clients.OthersInGroup($"call_{callId}").SendAsync("UserJoinedCall", Context.UserIdentifier);
    }

    public async Task LeaveCall(string callId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"call_{callId}");
        await Clients.OthersInGroup($"call_{callId}").SendAsync("UserLeftCall", Context.UserIdentifier);
    }

    public async Task SendSignal(string targetUserId, string signal)
    {
        // Signal is usually a JSON string containing SDP or ICE candidate
        await Clients.User(targetUserId).SendAsync("ReceiveSignal", Context.UserIdentifier, signal);
    }

    public async Task SendSpeakingState(bool isSpeaking)
    {
        // We could track which call the user is in, but for mesh, sending to all "call_..." groups works
        // or just let clients handle it if they're in the same group.
        // A better way is to find the group the user is currently in.
        // For simplicity, we'll assume the client knows their callId.
    }

    // Improved version: clients pass the callId
    public async Task SendSpeakingStateInCall(string callId, bool isSpeaking)
    {
        await Clients.OthersInGroup($"call_{callId}").SendAsync("UserSpeaking", Context.UserIdentifier, isSpeaking);
    }

    public async Task SendMuteState(string callId, bool isMuted)
    {
        await Clients.OthersInGroup($"call_{callId}").SendAsync("UserMuted", Context.UserIdentifier, isMuted);
    }

    public async Task SendCameraState(string callId, bool hasVideo)
    {
        await Clients.OthersInGroup($"call_{callId}").SendAsync("UserCameraChanged", Context.UserIdentifier, hasVideo);
    }

    public async Task BroadcastHuddleMessage(string callId)
    {
        // Simply notify others to reload thread messages
        await Clients.OthersInGroup($"call_{callId}").SendAsync("NewHuddleMessage");
    }

    public async Task BroadcastCallStarted(string channelId, string callId)
    {
        // Notify everyone in the channel that a call has started
        await Clients.Group(channelId).SendAsync("CallStarted", channelId, callId);
    }

    public async Task BroadcastCallEnded(string channelId)
    {
        await Clients.Group(channelId).SendAsync("CallEnded", channelId);
    }
}
