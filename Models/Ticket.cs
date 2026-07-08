namespace HelpDesk.Models;

public class Ticket
{
    public int Id { get; set; }
    public string RequesterName { get; set; } = "";
    public string RequesterEmail { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "Other";
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<TicketComment> Comments { get; set; } = new();
}
