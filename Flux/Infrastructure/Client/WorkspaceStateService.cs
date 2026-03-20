namespace Flux.Infrastructure.Client;

/// <summary>
/// Manages the state of the current workspace and its channels.
/// Shared across layout and pages.
/// </summary>
public class WorkspaceStateService
{
    private readonly WorkspaceClientService _workspaceService;

    public WorkspaceStateService(WorkspaceClientService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public Guid? CurrentWorkspaceId { get; private set; }
    public string? CurrentWorkspaceName { get; private set; }
    public List<ChannelSummary> Channels { get; private set; } = new();
    public List<MemberDto> Members { get; private set; } = new();

    public List<NotificationDto> Notifications { get; private set; } = new();
    public int UnreadNotificationCount => Notifications.Count(n => !n.IsRead);

    public event Action? OnStateChanged;

    /// Loads channels for a workspace and updates the state.
    public async Task LoadWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        if (CurrentWorkspaceId == workspaceId && Channels.Count > 0) return;

        CurrentWorkspaceId = workspaceId;
        
        // Load workspace details to get the name
        var workspacesResult = await _workspaceService.GetWorkspacesAsync(userId);
        if (workspacesResult.IsSuccess && workspacesResult.Value != null)
        {
            var currentWorkspace = workspacesResult.Value.FirstOrDefault(w => w.Id == workspaceId);
            CurrentWorkspaceName = currentWorkspace?.Name;
            NotifyStateChanged();
        }

        await RefreshChannelsAsync(userId);
        await RefreshMembersAsync(userId);
    }

    public void AddNotification(NotificationDto notification)
    {
        Notifications.Insert(0, notification);
        NotifyStateChanged();
    }

    public void MarkNotificationAsRead(Guid messageId)
    {
        var note = Notifications.FirstOrDefault(n => n.MessageId == messageId);
        if (note != null && !note.IsRead)
        {
            note.IsRead = true;
            NotifyStateChanged();
        }
    }

    public void MarkAllNotificationsAsRead()
    {
        bool changed = false;
        foreach (var n in Notifications.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            changed = true;
        }
        if (changed) NotifyStateChanged();
    }

    public void ClearNotifications()
    {
        Notifications.Clear();
        NotifyStateChanged();
    }

    public async Task RefreshMembersAsync(Guid userId)
    {
        if (CurrentWorkspaceId == null) return;
        
        var result = await _workspaceService.GetWorkspaceMembersAsync(CurrentWorkspaceId.Value, userId);
        if (result.IsSuccess)
        {
            Members = result.Value ?? new();
        }
        else
        {
            Members = new List<MemberDto>();
        }
        
        NotifyStateChanged();
    }

    public async Task RefreshChannelsAsync(Guid userId)
    {
        if (CurrentWorkspaceId == null) return;
        
        var result = await _workspaceService.GetChannelsAsync(CurrentWorkspaceId.Value, userId);
        if (result.IsSuccess)
        {
            Channels = result.Value ?? new();
        }
        else
        {
            Channels = new List<ChannelSummary>();
        }
        
        NotifyStateChanged();
    }

    public void UpdateMemberPresence(Guid userId, string status)
    {
        var memberIndex = Members.FindIndex(m => m.Id == userId);
        if (memberIndex >= 0)
        {
            var member = Members[memberIndex];
            Members[memberIndex] = member with { Status = status };
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
