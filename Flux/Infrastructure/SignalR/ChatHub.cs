using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Flux.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Flux.Infrastructure.SignalR;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<string, int> _userConnections = new();
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
                        user.Status = "Online";
                        await dbContext.SaveChangesAsync();
                    }
                }
                
                await Clients.All.SendAsync("UserPresenceChanged", userId, "Online");
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
}
