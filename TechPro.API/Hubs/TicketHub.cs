using Microsoft.AspNetCore.SignalR;

namespace TechPro.API.Hubs
{
    public class TicketHub : Hub
    {
        public async Task NotifyTicketStatusChanged(string ticketId, string status)
        {
            await Clients.All.SendAsync("TicketStatusChanged", ticketId, status);
        }

        public async Task NotifyTicketCreated(string ticketId)
        {
            await Clients.All.SendAsync("TicketCreated", ticketId);
        }

        public async Task BroadcastChatMessage(string ticketId, string userName, string message, string time)
        {
            await Clients.All.SendAsync("ChatMessage", ticketId, userName, message, time);
        }

        public async Task BroadcastScratchUpdated(string ticketId)
        {
            await Clients.All.SendAsync("ScratchUpdated", ticketId);
        }

        public async Task SendNotification(string userId, string title, string message, string type = "info", string? link = null)
        {
            await Clients.User(userId).SendAsync("NewNotification", new
            {
                title,
                message,
                type,
                link,
                createdAt = DateTime.Now
            });
        }
    }
}

