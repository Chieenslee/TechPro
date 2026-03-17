using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TechPro.Models.InternalChat;
using TechPro.Services;

namespace TechPro.Hubs
{
    [Authorize]
    public class InternalChatHub : Hub
    {
        private readonly InternalChatStore _store;

        public InternalChatHub(InternalChatStore store)
        {
            _store = store;
        }

        private (string userId, string name, string role, string? tenantId) UserContext()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.ConnectionId;
            var name = Context.User?.FindFirstValue("FullName")
                       ?? Context.User?.FindFirstValue(ClaimTypes.Name)
                       ?? "Unknown";
            var role = Context.User?.FindFirstValue(ClaimTypes.Role) ?? "Support";
            var tenantId = Context.User?.FindFirstValue("TenantId");
            return (userId, name, role, tenantId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _store.TrackDisconnect(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public Task<List<ChatChannelDto>> GetMyChannels()
        {
            var channels = _store.BuildChannelsForUser(Context.User!);
            return Task.FromResult(channels.ToList());
        }

        public Task<List<ChatMessageDto>> GetRecent(string channelId, int take = 50)
        {
            var channels = _store.BuildChannelsForUser(Context.User!);
            var channel = channels.FirstOrDefault(c => c.Id == channelId);
            if (channel == null || !_store.CanAccessChannel(Context.User!, channel))
            {
                return Task.FromResult(new List<ChatMessageDto>());
            }

            return Task.FromResult(_store.GetRecent(channelId, take).ToList());
        }

        public async Task JoinChannel(string channelId)
        {
            var channels = _store.BuildChannelsForUser(Context.User!);
            var channel = channels.FirstOrDefault(c => c.Id == channelId);
            if (channel == null || !_store.CanAccessChannel(Context.User!, channel))
            {
                throw new HubException("FORBIDDEN");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            var (userId, name, role, tenantId) = UserContext();
            _store.TrackJoin(Context.ConnectionId, $"{userId}:{role}:{tenantId}", channelId);

            await Clients.Group(channelId).SendAsync("PresenceChanged", channelId, _store.GetOnlineCount(channelId));
        }

        public async Task LeaveChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            _store.TrackLeave(Context.ConnectionId, channelId);
            await Clients.Group(channelId).SendAsync("PresenceChanged", channelId, _store.GetOnlineCount(channelId));
        }

        public async Task Typing(string channelId, bool isTyping)
        {
            var channels = _store.BuildChannelsForUser(Context.User!);
            var channel = channels.FirstOrDefault(c => c.Id == channelId);
            if (channel == null || !_store.CanAccessChannel(Context.User!, channel)) return;

            var (_, name, role, _) = UserContext();
            await Clients.OthersInGroup(channelId).SendAsync("Typing", channelId, name, role, isTyping);
        }

        public async Task SendMessage(string channelId, string body)
        {
            body = (body ?? "").Trim();
            if (string.IsNullOrEmpty(body)) return;
            if (body.Length > 2000) body = body[..2000];

            var channels = _store.BuildChannelsForUser(Context.User!);
            var channel = channels.FirstOrDefault(c => c.Id == channelId);
            if (channel == null || !_store.CanAccessChannel(Context.User!, channel))
            {
                throw new HubException("FORBIDDEN");
            }

            var (userId, name, role, tenantId) = UserContext();
            var msg = new ChatMessageDto(
                Id: Guid.NewGuid().ToString("N"),
                ChannelId: channelId,
                SenderId: userId,
                SenderName: name,
                SenderRole: role,
                SenderTenantId: tenantId ?? "",
                Body: body,
                SentAt: DateTimeOffset.Now
            );

            _store.AddMessage(msg);

            await Clients.Group(channelId).SendAsync("Message", msg);
        }
    }
}

