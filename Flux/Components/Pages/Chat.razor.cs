using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Flux.Features.Messages.EditMessage;
using Flux.Features.Messages.SendMessage;
using Flux.Features.Messages.GetMessages;
using Flux.Infrastructure.Client;

namespace Flux.Components.Pages;

public partial class Chat : ComponentBase, IAsyncDisposable
{
    [Inject] private MessageClientService MessageService { get; set; } = default!;
    [Inject] private UploadClientService UploadService { get; set; } = default!;
    [Inject] private WorkspaceStateService StateService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private Flux.Infrastructure.Services.IToastService ToastService { get; set; } = default!;

    [Parameter] public Guid WorkspaceId { get; set; }
    [Parameter] public Guid ChannelId { get; set; }

    private List<MessageDto> _messages = new();
    private List<MessageDto> RootMessages => _messages.Where(m => m.ParentMessageId == null).OrderBy(m => m.CreatedAt).ToList();
    private string _newMessageContent = "";
    private string? _currentChannelName;
    private bool _isLoading = true;
    private bool _isLoadingMore = false;
    private bool _hasMoreMessages = true;
    private ElementReference _messagesViewport;
    private Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize<MessageDto>? _messageVirtualizeRef;
    private ElementReference _textAreaRef;
    private DotNetObjectReference<Chat>? _objRef;
    private bool _shouldScrollToBottom = false;
    private HubConnection? _hubConnection;

    // Thread specific state
    private MessageDto? _activeThreadParent;
    private string _newThreadMessageContent = "";
    private ElementReference _threadViewport;
    private ElementReference _threadTextAreaRef;

    // Profile specific state
    private MemberDto? _activeProfile;

    private async Task OpenThread(MessageDto parent)
    {
        _activeProfile = null;
        _activeThreadParent = parent;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(100);
        await JS.InvokeVoidAsync("scrollToBottom", _threadViewport);
    }

    private void CloseThread()
    {
        _activeThreadParent = null;
    }

    private void OpenProfile(Guid userId)
    {
        _activeThreadParent = null;
        _activeProfile = StateService.Members.FirstOrDefault(m => m.Id == userId);
        StateHasChanged();
    }

    private void CloseProfile()
    {
        _activeProfile = null;
    }

    private void OnThreadInputChanged(string value)
    {
        _newThreadMessageContent = value;
    }

    private async Task SendThreadMessage()
    {
        if (string.IsNullOrWhiteSpace(_newThreadMessageContent) || _activeThreadParent == null) return;

        var content = _newThreadMessageContent.Trim();
        _newThreadMessageContent = string.Empty;

        var command = new SendMessageCommand(content, ChannelId, CurrentUserId, _activeThreadParent.Id);
        var result = await MessageService.SendMessageAsync(command);

        if (!result.IsSuccess)
        {
            _newThreadMessageContent = content;
            ToastService.ShowError($"Failed to send reply: {result.Error}");
        }
        else
        {
            // The message will be added via SignalR
            await JS.InvokeVoidAsync("scrollToBottom", _threadViewport);
        }
    }

    // Mentions logic
    private bool _showMentionsList = false;
    private List<MemberDto> _filteredMembers = new();
    private Guid _selectedMentionId;
    private string _mentionSearchQuery = "";
    
    private void OnChatInputChanged(string value)
    {
        _newMessageContent = value;
        CheckForMentions();
    }

    private void CheckForMentions()
    {
        if (string.IsNullOrEmpty(_newMessageContent))
        {
            _showMentionsList = false;
            return;
        }

        var lastAtSymbol = _newMessageContent.LastIndexOf('@');
        if (lastAtSymbol >= 0)
        {
            if (lastAtSymbol == 0 || _newMessageContent[lastAtSymbol - 1] == ' ' || _newMessageContent[lastAtSymbol - 1] == '\n')
            {
                _mentionSearchQuery = _newMessageContent.Substring(lastAtSymbol + 1);
                
                if (!_mentionSearchQuery.Contains(" "))
                {
                    _filteredMembers = StateService.Members
                        .Where(m => m.Username.Contains(_mentionSearchQuery, StringComparison.OrdinalIgnoreCase))
                        .Take(5)
                        .ToList();

                    if (_filteredMembers.Any())
                    {
                        _showMentionsList = true;
                        _selectedMentionId = _filteredMembers.First().Id;
                        return;
                    }
                }
            }
        }
        
        _showMentionsList = false;
    }

    private void InsertMention(string username)
    {
        var lastAtSymbol = _newMessageContent.LastIndexOf('@');
        if (lastAtSymbol >= 0)
        {
            var beforeMention = _newMessageContent.Substring(0, lastAtSymbol);
            _newMessageContent = $"{beforeMention}@{username} ";
        }
        _showMentionsList = false;
        StateHasChanged();
    }

    private Guid CurrentUserId;
    
    // --- Toolbar Actions ---
    private async Task HandleFileUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;
        
        ToastService.ShowInfo("Uploading image...");
        try 
        {
            using var stream = file.OpenReadStream(10 * 1024 * 1024); // 10MB max
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(memoryStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            var result = await UploadService.UploadImageAsync(content);
            if (result.IsSuccess)
            {
                _newMessageContent += (string.IsNullOrEmpty(_newMessageContent) ? "" : "\n") + $"[image: {result.Value}]";
                ToastService.ShowSuccess("Image attached.");
                StateHasChanged();
            }
            else
            {
                ToastService.ShowError("Upload failed: " + result.Error);
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError("Upload error: " + ex.Message);
        }
    }

    // Audio Recorder State
    private bool _isAudioRecording = false;
    private string _audioDurationString = "00:00";
    private int _audioRecordingSeconds = 0;
    private System.Timers.Timer? _audioTimer;

    private async Task StartAudioRecording()
    {
        var started = await JS.InvokeAsync<bool>("window.voiceRecorder.startRecording", _objRef);
        if (started)
        {
            _isAudioRecording = true;
            _audioRecordingSeconds = 0;
            _audioDurationString = "00:00";
            
            _audioTimer?.Stop();
            _audioTimer?.Dispose();
            
            _audioTimer = new System.Timers.Timer(1000);
            _audioTimer.Elapsed += (sender, e) =>
            {
                _audioRecordingSeconds++;
                var ts = TimeSpan.FromSeconds(_audioRecordingSeconds);
                _audioDurationString = ts.ToString(@"mm\:ss");
                InvokeAsync(StateHasChanged);
            };
            _audioTimer.Start();
            StateHasChanged();
        }
        else
        {
            ToastService.ShowError("Could not access microphone.");
        }
    }

    private async Task StopAudioRecording()
    {
        _audioTimer?.Stop();
        await JS.InvokeVoidAsync("window.voiceRecorder.stopRecording");
        _isAudioRecording = false;
        StateHasChanged();
    }

    private async Task CancelAudioRecording()
    {
        _audioTimer?.Stop();
        await JS.InvokeVoidAsync("window.voiceRecorder.cancelRecording");
        _isAudioRecording = false;
        StateHasChanged();
    }

    [JSInvokable]
    public async Task OnAudioDataReceived(string base64Data)
    {
        if (string.IsNullOrEmpty(base64Data)) return;
        
        ToastService.ShowInfo("Uploading audio...");
        
        try
        {
            var audioBytes = Convert.FromBase64String(base64Data);
            using var content = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(audioBytes);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/webm");
            content.Add(byteContent, "file", $"voice_{DateTime.UtcNow:yyyyMMddHHmmss}.webm");

            var result = await UploadService.UploadAudioAsync(content);
            if (result.IsSuccess)
            {
                _newMessageContent += (string.IsNullOrEmpty(_newMessageContent) ? "" : "\n") + $"[audio: {result.Value}]";
                ToastService.ShowSuccess("Audio recorded.");
                await SendMessage();
            }
            else
            {
                ToastService.ShowError("Audio upload failed: " + result.Error);
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError("Error processing audio: " + ex.Message);
        }
        
        StateHasChanged();
    }

    private void TriggerMention()
    {
        _newMessageContent += "@";
        CheckForMentions();
        _textAreaRef.FocusAsync();
        StateHasChanged();
    }

    private void ToggleFormatting()
    {
        ToastService.ShowInfo("Rich text formatting coming soon!");
    }

    private void FlagAsImportant()
    {
        if (!_newMessageContent.StartsWith("IMPORTANT: "))
        {
            _newMessageContent = "IMPORTANT: " + _newMessageContent;
        }
        ToastService.ShowInfo("Message flagged as important");
        StateHasChanged();
    }
    
    // Video Recorder State
    private bool _showVideoRecorder = false;
    private bool _isCameraError = false;
    private bool _isRecording = false;
    private string? _recordedVideoUrl = null;
    private string _currentCaptureMode = "camera";
    private bool _isCameraEnabled = true;
    private bool _isMicEnabled = true;

    private System.Timers.Timer? _recordingTimer;
    private int _recordingSeconds = 0;
    private string _recordingDurationString = "00:00";
    
    private int _countdownSeconds = 0;
    private bool _isCountingDown = false;

    private async Task OpenVideoRecorder()
    {
        _showVideoRecorder = true;
        _isCameraError = false;
        _isRecording = false;
        _recordedVideoUrl = null;
        _currentCaptureMode = "camera";
        _isCameraEnabled = true;
        _isMicEnabled = true;
        _recordingSeconds = 0;
        _recordingDurationString = "00:00";
        StateHasChanged();

        await Task.Delay(100);
        await SwitchCaptureMode("camera");
    }

    private async Task ToggleCamera()
    {
        _isCameraEnabled = !_isCameraEnabled;
        await JS.InvokeVoidAsync("window.videoRecorder.toggleVideo", _isCameraEnabled);
    }

    private async Task ToggleMic()
    {
        _isMicEnabled = !_isMicEnabled;
        await JS.InvokeVoidAsync("window.videoRecorder.toggleAudio", _isMicEnabled);
    }

    private async Task SwitchCaptureMode(string mode)
    {
        _currentCaptureMode = mode;
        _isCameraError = false;
        StateHasChanged();
        
        bool started;
        if (mode == "camera")
        {
            started = await JS.InvokeAsync<bool>("window.videoRecorder.startCamera", "videoPreview");
        }
        else
        {
            started = await JS.InvokeAsync<bool>("window.videoRecorder.startScreenShare", "videoPreview");
        }

        if (!started)
        {
            _isCameraError = true;
            StateHasChanged();
        }
        else
        {
            await JS.InvokeVoidAsync("window.videoRecorder.toggleVideo", _isCameraEnabled);
            await JS.InvokeVoidAsync("window.videoRecorder.toggleAudio", _isMicEnabled);
        }
    }

    private async Task CloseVideoRecorder()
    {
        _showVideoRecorder = false;
        if (_isRecording)
        {
            await StopRecording();
        }
        await JS.InvokeVoidAsync("window.videoRecorder.stopCamera");
    }

    private async Task StartRecording()
    {
        if (_isCountingDown) return;
        
        _isCountingDown = true;
        _countdownSeconds = 3;
        StateHasChanged();

        while (_countdownSeconds > 0)
        {
            await Task.Delay(1000);
            _countdownSeconds--;
            StateHasChanged();
        }
        
        _isCountingDown = false;

        var started = await JS.InvokeAsync<bool>("window.videoRecorder.startRecording");
        if (started)
        {
            _isRecording = true;
            _recordingSeconds = 0;
            _recordingDurationString = "00:00";
            
            if (_recordingTimer != null)
            {
                _recordingTimer.Stop();
                _recordingTimer.Dispose();
            }
            
            _recordingTimer = new System.Timers.Timer(1000);
            _recordingTimer.Elapsed += (sender, e) =>
            {
                _recordingSeconds++;
                var ts = TimeSpan.FromSeconds(_recordingSeconds);
                _recordingDurationString = ts.ToString(@"mm\:ss");
                InvokeAsync(StateHasChanged);
            };
            _recordingTimer.Start();
        }
    }

    private async Task StopRecording()
    {
        _isRecording = false;
        if (_recordingTimer != null)
        {
            _recordingTimer.Stop();
            _recordingTimer.Dispose();
            _recordingTimer = null;
        }
        _recordedVideoUrl = await JS.InvokeAsync<string>("window.videoRecorder.stopRecording", "videoPreview");
    }

    private async Task RetakeVideo()
    {
        _recordedVideoUrl = null;
        await JS.InvokeVoidAsync("window.videoRecorder.startCamera", "videoPreview");
    }

    private async Task DownloadVideo()
    {
        if (_recordedVideoUrl != null)
        {
            await JS.InvokeVoidAsync("window.videoRecorder.downloadVideo", _recordedVideoUrl);
        }
    }

    private bool _isUploading = false;

    private async Task UploadVideo()
    {
        if (_recordedVideoUrl == null) return;

        _isUploading = true;
        StateHasChanged();

        var uploadedUrl = await JS.InvokeAsync<string>("window.videoRecorder.uploadVideo", _recordedVideoUrl, "/api/uploads/video");

        _isUploading = false;

        if (!string.IsNullOrEmpty(uploadedUrl))
        {
            ToastService.ShowSuccess("Video ready to send!");
            _newMessageContent += (string.IsNullOrEmpty(_newMessageContent) ? "" : "\n") + $"[video: {uploadedUrl}]";
            await CloseVideoRecorder();
        }
        else
        {
            ToastService.ShowError("Failed to upload video.");
        }
        StateHasChanged();
    }

    private string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity is { IsAuthenticated: true })
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
            if (idClaim != null && Guid.TryParse(idClaim.Value, out var id))
            {
                CurrentUserId = id;
            }
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/chatHub"), options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    try
                    {
                        var token = await JS.InvokeAsync<string>("sessionStorage.getItem", "authToken");
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            token = await JS.InvokeAsync<string>("localStorage.getItem", "authToken");
                        }
                        return token;
                    }
                    catch
                    {
                        return null;
                    }
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<SendMessageResponse>("ReceiveMessage", async (response) =>
        {
            if (response.ChannelId == ChannelId)
            {
                if (!_messages.Any(m => m.Id == response.Id))
                {
                    _messages.Add(new MessageDto(
                        response.Id, 
                        response.Content, 
                        response.UserId, 
                        response.Username, 
                        response.CreatedAt, 
                        null,
                        response.AvatarUrl,
                        response.ParentMessageId,
                        0));
                    
                    // Increment parent reply count if this is a reply
                    if (response.ParentMessageId != null)
                    {
                        var parentIndex = _messages.FindIndex(m => m.Id == response.ParentMessageId);
                        if (parentIndex != -1)
                        {
                            var parent = _messages[parentIndex];
                            _messages[parentIndex] = parent with { ReplyCount = parent.ReplyCount + 1 };
                        }
                    }

                    _shouldScrollToBottom = true;
                    await InvokeAsync(StateHasChanged);
                }
            }
        });

        _hubConnection.On<EditMessageResponse>("MessageUpdated", async (response) =>
        {
            var index = _messages.FindIndex(m => m.Id == response.MessageId);
            if (index != -1)
            {
                _messages[index] = _messages[index] with { Content = response.Content, UpdatedAt = response.UpdatedAt };
                await InvokeAsync(StateHasChanged);
            }
        });

        _hubConnection.On<Guid>("MessageDeleted", async (messageId) =>
        {
            var index = _messages.FindIndex(m => m.Id == messageId);
            if (index != -1)
            {
                _messages.RemoveAt(index);
                await InvokeAsync(StateHasChanged);
            }
        });

        _hubConnection.On<ToggleReactionResponseDto>("ReactionToggled", async (response) =>
        {
            var msg = _messages.FirstOrDefault(m => m.Id == response.MessageId);
            if (msg != null)
            {
                var newReactions = msg.Reactions != null ? new List<ReactionDto>(msg.Reactions) : new List<ReactionDto>();
                if (response.IsAdded)
                {
                    if (!newReactions.Any(r => r.Emoji == response.Emoji && r.UserId == response.UserId))
                    {
                        newReactions.Add(new ReactionDto(response.UserId, response.Emoji));
                    }
                }
                else
                {
                    newReactions.RemoveAll(r => r.Emoji == response.Emoji && r.UserId == response.UserId);
                }
                
                var index = _messages.IndexOf(msg);
                _messages[index] = msg with { Reactions = newReactions };
                
                await InvokeAsync(async () => 
                {
                    StateHasChanged();
                    if (_messageVirtualizeRef != null)
                    {
                        await _messageVirtualizeRef.RefreshDataAsync();
                    }
                });
            }
        });

        _hubConnection.On<string, string>("UserPresenceChanged", async (userId, status) =>
        {
            if (Guid.TryParse(userId, out var uid))
            {
                StateService.UpdateMemberPresence(uid, status);
                await InvokeAsync(StateHasChanged);
            }
        });

        await _hubConnection.StartAsync();
    }

    private Guid _currentConnectedChannelId;

    protected override async Task OnParametersSetAsync()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            if (_currentConnectedChannelId != Guid.Empty && _currentConnectedChannelId != ChannelId)
            {
                await _hubConnection.InvokeAsync("LeaveChannel", _currentConnectedChannelId.ToString());
            }

            if (_currentConnectedChannelId != ChannelId)
            {
                await _hubConnection.InvokeAsync("JoinChannel", ChannelId.ToString());
                _currentConnectedChannelId = ChannelId;
            }
        }
        else
        {
            _currentConnectedChannelId = ChannelId;
        }

        await LoadChatData();
    }

    private async Task LoadChatData()
    {
        _isLoading = true;
        _messages.Clear();
        _hasMoreMessages = true;
        try
        {
            var channel = StateService.Channels.FirstOrDefault(c => c.Id == ChannelId);
            _currentChannelName = channel?.Name ?? "general"; 
            
            var result = await MessageService.GetMessagesAsync(ChannelId);
            if (result.IsSuccess)
            {
                _messages = result.Value ?? new();
                _hasMoreMessages = _messages.Count >= 50;
                _shouldScrollToBottom = true;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadMoreMessages()
    {
        if (_isLoadingMore || !_hasMoreMessages || !_messages.Any()) return;

        _isLoadingMore = true;
        try
        {
            var oldestMessageTimestamp = _messages.First().CreatedAt;
            var result = await MessageService.GetMessagesAsync(ChannelId, oldestMessageTimestamp);
            
            if (result.IsSuccess && result.Value != null && result.Value.Any())
            {
                var olderMessages = result.Value;
                _hasMoreMessages = olderMessages.Count >= 50;
                _messages.InsertRange(0, olderMessages);
            }
            else
            {
                _hasMoreMessages = false;
            }
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
        }

        if (_shouldScrollToBottom)
        {
            _shouldScrollToBottom = false;
            await ScrollToBottom();
        }
    }

    private async Task ScrollToBottom()
    {
        await JS.InvokeVoidAsync("scrollToBottom", _messagesViewport);
    }

    [JSInvokable]
    public async Task SendMessageFromJS()
    {
        await SendMessage();
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private async Task HandleThreadKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendThreadMessage();
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_newMessageContent)) return;

        var content = _newMessageContent.Trim();
        _newMessageContent = string.Empty;

        await InvokeAsync(StateHasChanged);

        var command = new SendMessageCommand(content, ChannelId, CurrentUserId);
        var result = await MessageService.SendMessageAsync(command);

        if (!result.IsSuccess)
        {
            _newMessageContent = content;
            ToastService.ShowError($"Failed to send message: {result.Error}");
        }
        
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _objRef?.Dispose();
        if (_hubConnection != null)
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("LeaveChannel", ChannelId.ToString());
                }
            }
            catch { }
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    // --- Message Actions Logic ---
    private bool _showMessageMenu = false;
    private double _menuX = 0;
    private double _menuY = 0;
    private MessageDto? _selectedMessage;

    private Guid _editingMessageId;
    private string _editMessageContent = "";

    private void ShowMessageMenu(MouseEventArgs e, MessageDto message)
    {
        _selectedMessage = message;
        _menuX = e.ClientX;
        _menuY = e.ClientY;
        _showMessageMenu = true;
    }

    private void CloseMessageMenu()
    {
        _showMessageMenu = false;
        _selectedMessage = null;
    }

    private async Task HandleQuickAction(MessageDto msg, string action)
    {
        _selectedMessage = msg;
        await HandleAction(action);
    }

    private async Task HandleAction(string action)
    {
        if (_selectedMessage == null) return;
        
        var messageId = _selectedMessage.Id;
        var content = _selectedMessage.Content;
        
        CloseMessageMenu();

        switch (action)
        {
            case "Reply":
                await OpenThread(_selectedMessage);
                break;
            case "Edit":
                _editingMessageId = messageId;
                _editMessageContent = content;
                break;
            case "Delete":
                await ConfirmDeleteMessage(messageId);
                break;
        }
    }

    private void CancelEdit()
    {
        _editingMessageId = Guid.Empty;
        _editMessageContent = "";
        StateHasChanged();
    }

    private async Task SaveEditMessage()
    {
        if (string.IsNullOrWhiteSpace(_editMessageContent)) return;
        
        var request = new Flux.Features.Messages.EditMessage.EditMessageRequest(_editMessageContent, CurrentUserId);
        var result = await MessageService.EditMessageAsync(_editingMessageId, request);
        
        if (result.IsSuccess)
        {
            var index = _messages.FindIndex(m => m.Id == _editingMessageId);
            if (index != -1)
            {
                _messages[index] = _messages[index] with { Content = _editMessageContent, UpdatedAt = DateTime.UtcNow };
            }
            ToastService.ShowSuccess("Message updated.");
        }
        else
        {
            ToastService.ShowError($"Failed to edit message: {result.Error}");
        }
        
        _editingMessageId = Guid.Empty;
        await InvokeAsync(StateHasChanged);
    }

    private bool _showDeleteDialog = false;
    private Guid _messageToDeleteId;

    private async Task ConfirmDeleteMessage(Guid messageId)
    {
        _messageToDeleteId = messageId;
        _showDeleteDialog = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ExecuteDeleteMessage()
    {
        var result = await MessageService.DeleteMessageAsync(_messageToDeleteId, CurrentUserId);
        if (result.IsSuccess)
        {
            _messages.RemoveAll(m => m.Id == _messageToDeleteId);
            ToastService.ShowSuccess("Message deleted.");
        }
        else
        {
            ToastService.ShowError($"Failed to delete message: {result.Error}");
        }
        
        _showDeleteDialog = false;
        await InvokeAsync(StateHasChanged);
    }

    private bool _showEmojiPicker = false;
    private double _emojiPickerX = 0;
    private double _emojiPickerY = 0;
    private Guid _emojiTargetMessageId;

    private async Task OpenEmojiPicker(MouseEventArgs e, Guid messageId = default)
    {
        _emojiTargetMessageId = messageId;
        _emojiPickerX = e.ClientX;
        
        // Dynamic Y position: if click is in the bottom half of the screen, show picker above
        _emojiPickerY = e.ClientY;
        
        // If the click is low on the screen, shift the picker up by its estimated height (350px for the new picker)
        if (e.ClientY > 450) 
        {
            _emojiPickerY = e.ClientY - 380;
        }
        else 
        {
            _emojiPickerY = e.ClientY + 20;
        }

        _showEmojiPicker = true;
        
        // Wait for the UI to render the <emoji-picker> then init JS
        await Task.Delay(50);
        await JS.InvokeVoidAsync("emojiPicker.init", _objRef, "main-emoji-picker");
    }

    private void CloseEmojiPicker()
    {
        _showEmojiPicker = false;
    }

    [JSInvokable]
    public async Task OnEmojiSelectedJS(string emoji)
    {
        var targetId = _emojiTargetMessageId;
        CloseEmojiPicker();

        if (targetId != default)
        {
            await ToggleReaction(targetId, emoji);
        }
        else
        {
            _newMessageContent += emoji;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ToggleReaction(Guid messageId, string emoji)
    {
        // Optimistic UI update for immediate feedback when clicking existing badges
        var msg = _messages.FirstOrDefault(m => m.Id == messageId);
        if (msg != null)
        {
            var newReactions = msg.Reactions != null ? new List<ReactionDto>(msg.Reactions) : new List<ReactionDto>();
            var existing = newReactions.FirstOrDefault(r => r.Emoji == emoji && r.UserId == CurrentUserId);
            
            if (existing != null) newReactions.Remove(existing);
            else newReactions.Add(new ReactionDto(CurrentUserId, emoji));
            
            var index = _messages.IndexOf(msg);
            _messages[index] = msg with { Reactions = newReactions };
            
            StateHasChanged();
            if (_messageVirtualizeRef != null)
            {
                await _messageVirtualizeRef.RefreshDataAsync();
            }
        }

        var result = await MessageService.ToggleReactionAsync(messageId, CurrentUserId, emoji);
        if (!result.IsSuccess)
        {
            ToastService.ShowError("Failed to toggle reaction.");
            // Revert optimistic update if failed? (Optional, usually SignalR will correct it)
        }
    }

    public record ToggleReactionResponseDto(Guid MessageId, string Emoji, Guid UserId, bool IsAdded);
}