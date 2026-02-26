using Flux.Infrastructure.Client;

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
    public List<ChannelSummary> Channels { get; private set; } = new();
    
    public event Action? OnStateChanged;

    /// <summary>
    /// Loads channels for a workspace and updates the state.
    /// </summary>
    public async Task LoadWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        if (CurrentWorkspaceId == workspaceId && Channels.Count > 0) return;

        CurrentWorkspaceId = workspaceId;
        Channels = await _fluxService.GetChannelsAsync(workspaceId, userId);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
