namespace TechPro.API.Services
{
    public class EmailOptions
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? Username { get; set; }
        public string? Password { get; set; }

        public string? FromEmail { get; set; }
        public string? FromName { get; set; } = "TechPro";

        // Demo: nếu không lấy được email khách, gửi về email này
        public string? DefaultTo { get; set; }

        public string SubjectTemplate { get; set; } = "[TechPro] Ticket {TicketId} is ready";
        public string BodyTemplate { get; set; } =
            "Hello {CustomerName},\n\nYour device ({DeviceName}) is repaired and ready for pickup.\nTicket: {TicketId}\nStatus: {Status}\n\nThank you,\nTechPro";
    }
}

