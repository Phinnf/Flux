using Microsoft.AspNetCore.SignalR;

namespace Flux.Infrastructure.SignalR;

public class ChatHub : Hub
{
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
