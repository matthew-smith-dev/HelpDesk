using System.Text.Json;
using HelpDesk.Models;

namespace HelpDesk.Services;

public class TicketService
{
    // The app stores tickets in a local JSON file for this MVP.
    // Later, this can be replaced with SQLite or SQL Server.
    private readonly string _dataFile = Path.Combine(AppContext.BaseDirectory, "data", "tickets.json");

    // This prevents two requests from writing to the JSON file at the same time.
    private readonly object _lock = new();

    public TicketService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dataFile)!);

        if (!File.Exists(_dataFile))
        {
            SaveTickets(CreateSampleTickets());
        }
    }

    public List<Ticket> GetAll()
    {
        lock (_lock)
        {
            return LoadTickets();
        }
    }

    public Ticket? GetById(int id)
    {
        lock (_lock)
        {
            return LoadTickets().FirstOrDefault(t => t.Id == id);
        }
    }

    public Ticket Create(Ticket ticket)
    {
        lock (_lock)
        {
            var tickets = LoadTickets();

            ticket.Id = tickets.Any() ? tickets.Max(t => t.Id) + 1 : 1;
            ticket.CreatedAt = DateTime.Now;
            ticket.UpdatedAt = DateTime.Now;

            tickets.Add(ticket);
            SaveTickets(tickets);

            return ticket;
        }
    }

    public bool UpdateStatus(int id, TicketStatus status)
    {
        lock (_lock)
        {
            var tickets = LoadTickets();
            var ticket = tickets.FirstOrDefault(t => t.Id == id);

            if (ticket is null)
            {
                return false;
            }

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.Now;

            SaveTickets(tickets);
            return true;
        }
    }

    public bool AddComment(int ticketId, TicketComment comment)
    {
        lock (_lock)
        {
            var tickets = LoadTickets();
            var ticket = tickets.FirstOrDefault(t => t.Id == ticketId);

            if (ticket is null)
            {
                return false;
            }

            comment.Id = ticket.Comments.Any() ? ticket.Comments.Max(c => c.Id) + 1 : 1;
            comment.CreatedAt = DateTime.Now;

            ticket.Comments.Add(comment);
            ticket.UpdatedAt = DateTime.Now;

            SaveTickets(tickets);
            return true;
        }
    }

    private List<Ticket> LoadTickets()
    {
        var json = File.ReadAllText(_dataFile);

        return JsonSerializer.Deserialize<List<Ticket>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Ticket>();
    }

    private void SaveTickets(List<Ticket> tickets)
    {
        var json = JsonSerializer.Serialize(tickets, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_dataFile, json);
    }

    // Sample tickets let the dashboard show useful data the first time the app runs.
    private static List<Ticket> CreateSampleTickets()
    {
        return new List<Ticket>
        {
            new Ticket
            {
                Id = 1,
                RequesterName = "Sample User",
                RequesterEmail = "sample.user@example.com",
                Title = "Cannot print to office printer",
                Description = "User reports that documents stay stuck in the print queue.",
                Category = "Printer",
                Priority = TicketPriority.High,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.Now.AddHours(-5),
                UpdatedAt = DateTime.Now.AddHours(-5),
                Comments = new List<TicketComment>
                {
                    new TicketComment
                    {
                        Id = 1,
                        Author = "Technician",
                        Message = "Initial ticket logged. Next step: check printer queue and print spooler service.",
                        CreatedAt = DateTime.Now.AddHours(-4)
                    }
                }
            },
            new Ticket
            {
                Id = 2,
                RequesterName = "Network User",
                RequesterEmail = "network.user@example.com",
                Title = "Wi-Fi disconnects randomly",
                Description = "Laptop disconnects from Wi-Fi several times during the day.",
                Category = "Network",
                Priority = TicketPriority.Medium,
                Status = TicketStatus.InProgress,
                CreatedAt = DateTime.Now.AddHours(-3),
                UpdatedAt = DateTime.Now.AddHours(-2)
            }
        };
    }
}
