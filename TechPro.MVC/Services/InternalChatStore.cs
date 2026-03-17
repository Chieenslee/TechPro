using System.Collections.Concurrent;
using System.Security.Claims;
using TechPro.Models.InternalChat;

namespace TechPro.Services
{
    public class InternalChatStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<ChatMessageDto>> _messagesByChannel = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _connectionsByChannel = new();
        private readonly ConcurrentDictionary<string, string> _userByConnection = new();
        private readonly object _presenceLock = new();

        private const int MaxMessagesPerChannel = 500;

        public IReadOnlyList<ChatChannelDto> BuildChannelsForUser(ClaimsPrincipal user)
        {
            var role = user.FindFirstValue(ClaimTypes.Role) ?? "Support";
            var tenantId = user.FindFirstValue("TenantId");

            // Channels cố định, có thể mở rộng sau này + lưu DB.
            var channels = new List<ChatChannelDto>
            {
                new(
                    Id: "global:announcements",
                    Name: "Thông báo toàn hệ thống",
                    Kind: "channel",
                    Scope: "global",
                    TenantId: null,
                    AllowedRoles: new[] { "SystemAdmin", "StoreAdmin", "Technician", "Support", "Storekeeper" },
                    Icon: "megaphone-fill",
                    Color: "#4f46e5"
                ),
                new(
                    Id: $"tenant:{tenantId}:ops",
                    Name: "Vận hành cửa hàng",
                    Kind: "channel",
                    Scope: "tenant",
                    TenantId: tenantId,
                    AllowedRoles: new[] { "StoreAdmin", "Support", "Technician", "Storekeeper", "SystemAdmin" },
                    Icon: "layers-fill",
                    Color: "#0ea5e9"
                ),
                new(
                    Id: $"tenant:{tenantId}:tech-fo",
                    Name: "Kỹ thuật ↔ Lễ tân",
                    Kind: "channel",
                    Scope: "tenant",
                    TenantId: tenantId,
                    AllowedRoles: new[] { "Support", "Technician", "StoreAdmin", "SystemAdmin" },
                    Icon: "headset",
                    Color: "#10b981"
                ),
                new(
                    Id: $"tenant:{tenantId}:inventory",
                    Name: "Kho & Phê duyệt linh kiện",
                    Kind: "channel",
                    Scope: "tenant",
                    TenantId: tenantId,
                    AllowedRoles: new[] { "Storekeeper", "StoreAdmin", "SystemAdmin", "Technician" },
                    Icon: "box-seam",
                    Color: "#f59e0b"
                ),
                new(
                    Id: "role:management",
                    Name: "Quản lý - phê duyệt gấp",
                    Kind: "channel",
                    Scope: "role",
                    TenantId: null,
                    AllowedRoles: new[] { "StoreAdmin", "SystemAdmin" },
                    Icon: "shield-lock-fill",
                    Color: "#ef4444"
                ),
            };

            return channels.Where(c => c.AllowedRoles.Contains(role)).ToList();
        }

        public bool CanAccessChannel(ClaimsPrincipal user, ChatChannelDto channel)
        {
            var role = user.FindFirstValue(ClaimTypes.Role) ?? "Support";
            if (!channel.AllowedRoles.Contains(role)) return false;

            if (channel.Scope == "tenant")
            {
                var tenantId = user.FindFirstValue("TenantId");
                return !string.IsNullOrEmpty(tenantId) && string.Equals(tenantId, channel.TenantId, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public void AddMessage(ChatMessageDto msg)
        {
            var q = _messagesByChannel.GetOrAdd(msg.ChannelId, _ => new ConcurrentQueue<ChatMessageDto>());
            q.Enqueue(msg);
            while (q.Count > MaxMessagesPerChannel && q.TryDequeue(out _)) { }
        }

        public IReadOnlyList<ChatMessageDto> GetRecent(string channelId, int take = 50)
        {
            if (!_messagesByChannel.TryGetValue(channelId, out var q)) return Array.Empty<ChatMessageDto>();
            return q.Reverse().Take(take).Reverse().ToList();
        }

        public void TrackJoin(string connectionId, string userKey, string channelId)
        {
            lock (_presenceLock)
            {
                _userByConnection[connectionId] = userKey;
                if (!_connectionsByChannel.TryGetValue(channelId, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _connectionsByChannel[channelId] = set;
                }
                set.Add(connectionId);
            }
        }

        public void TrackLeave(string connectionId, string channelId)
        {
            lock (_presenceLock)
            {
                if (_connectionsByChannel.TryGetValue(channelId, out var set))
                {
                    set.Remove(connectionId);
                }
            }
        }

        public void TrackDisconnect(string connectionId)
        {
            lock (_presenceLock)
            {
                _userByConnection.TryRemove(connectionId, out _);
                foreach (var kv in _connectionsByChannel)
                {
                    kv.Value.Remove(connectionId);
                }
            }
        }

        public int GetOnlineCount(string channelId)
        {
            lock (_presenceLock)
            {
                return _connectionsByChannel.TryGetValue(channelId, out var set) ? set.Count : 0;
            }
        }
    }
}

