namespace HelpDesk.Models;

public class TicketComment
{
    public int Id { get; set; }
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
