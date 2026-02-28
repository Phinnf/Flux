namespace Flux.Infrastructure.Client;

/// <summary>
/// Manages the state of the current workspace and its channels.
/// Shared across layout and pages.
/// </summary>
public class WorkspaceStateService
{
    private readonly FluxClientService _fluxService;

    public WorkspaceStateService(FluxClientService fluxService)
    {
        _fluxService = fluxService;
    }

    public Guid? CurrentWorkspaceId { get; private set; }
    public string? CurrentWorkspaceName { get; private set; }
    public List<ChannelSummary> Channels { get; private set; } = new();

    public event Action? OnStateChanged;

    /// Loads channels for a workspace and updates the state.
    public async Task LoadWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        if (CurrentWorkspaceId == workspaceId && Channels.Count > 0) return;

        CurrentWorkspaceId = workspaceId;
        
        // Load workspace details to get the name
        var workspacesResult = await _fluxService.GetWorkspacesAsync(userId);
        if (workspacesResult.IsSuccess && workspacesResult.Value != null)
        {
            var currentWorkspace = workspacesResult.Value.FirstOrDefault(w => w.Id == workspaceId);
            CurrentWorkspaceName = currentWorkspace?.Name;
        }

        await RefreshChannelsAsync(userId);
    }

    public async Task RefreshChannelsAsync(Guid userId)
    {
        if (CurrentWorkspaceId == null) return;
        
        var result = await _fluxService.GetChannelsAsync(CurrentWorkspaceId.Value, userId);
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

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
