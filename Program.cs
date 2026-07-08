using HelpDesk.Models;
using HelpDesk.Services;

var builder = WebApplication.CreateBuilder(args);

// TicketService is registered once and reused by the app.
// It handles creating, reading, updating, and saving tickets.
builder.Services.AddSingleton<TicketService>();

var app = builder.Build();

// Allows the app to load CSS files from the wwwroot folder.
app.UseStaticFiles();

// Dashboard route: shows ticket statistics and the main ticket table.
app.MapGet("/", (TicketService tickets) =>
{
    var allTickets = tickets.GetAll();
    var openCount = allTickets.Count(t => t.Status != TicketStatus.Closed);
    var criticalCount = allTickets.Count(t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Closed);
    var closedCount = allTickets.Count(t => t.Status == TicketStatus.Closed);

    var rows = string.Join("", allTickets
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => $"""
        <tr>
            <td>#{t.Id}</td>
            <td><a href="/tickets/{t.Id}">{System.Net.WebUtility.HtmlEncode(t.Title)}</a></td>
            <td>{t.Category}</td>
            <td><span class="badge priority-{t.Priority.ToString().ToLower()}">{t.Priority}</span></td>
            <td><span class="badge status-{t.Status.ToString().ToLower()}">{t.Status}</span></td>
            <td>{System.Net.WebUtility.HtmlEncode(t.RequesterName)}</td>
            <td>{t.CreatedAt:yyyy-MM-dd HH:mm}</td>
        </tr>
        """));

    var html = Html.Layout("Dashboard", $"""
        <section class="hero">
            <div>
                <h1>Help Desk</h1>
                <p>A simple IT help desk ticketing system for tracking user support requests.</p>
            </div>
            <a class="button" href="/tickets/new">Create New Ticket</a>
        </section>

        <section class="stats-grid">
            <div class="stat-card">
                <span class="stat-number">{allTickets.Count}</span>
                <span class="stat-label">Total Tickets</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{openCount}</span>
                <span class="stat-label">Open / In Progress</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{criticalCount}</span>
                <span class="stat-label">Critical Open</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{closedCount}</span>
                <span class="stat-label">Closed</span>
            </div>
        </section>

        <section class="panel">
            <h2>Tickets</h2>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Title</th>
                        <th>Category</th>
                        <th>Priority</th>
                        <th>Status</th>
                        <th>Requester</th>
                        <th>Created</th>
                    </tr>
                </thead>
                <tbody>
                    {(string.IsNullOrWhiteSpace(rows) ? "<tr><td colspan='7'>No tickets yet.</td></tr>" : rows)}
                </tbody>
            </table>
        </section>
    """);

    return Results.Content(html, "text/html");
});

// Shows the form used to create a new support ticket.
app.MapGet("/tickets/new", () =>
{
    var html = Html.Layout("Create Ticket", """
        <section class="panel narrow">
            <h1>Create New Ticket</h1>
            <form method="post" action="/tickets">
                <label>Requester Name</label>
                <input name="requesterName" required placeholder="Example: John Smith">

                <label>Requester Email</label>
                <input name="requesterEmail" type="email" required placeholder="Example: john@example.com">

                <label>Title</label>
                <input name="title" required placeholder="Example: Printer not printing">

                <label>Category</label>
                <select name="category">
                    <option>Hardware</option>
                    <option>Software</option>
                    <option>Network</option>
                    <option>Account Access</option>
                    <option>Printer</option>
                    <option>Other</option>
                </select>

                <label>Priority</label>
                <select name="priority">
                    <option>Low</option>
                    <option selected>Medium</option>
                    <option>High</option>
                    <option>Critical</option>
                </select>

                <label>Description</label>
                <textarea name="description" required placeholder="Describe the issue clearly..."></textarea>

                <div class="form-actions">
                    <a class="button secondary" href="/">Cancel</a>
                    <button class="button" type="submit">Create Ticket</button>
                </div>
            </form>
        </section>
    """);

    return Results.Content(html, "text/html");
});

// Receives the form data from /tickets/new and saves a new ticket.
app.MapPost("/tickets", async (HttpRequest request, TicketService tickets) =>
{
    var form = await request.ReadFormAsync();

    var ticket = new Ticket
    {
        RequesterName = form["requesterName"].ToString(),
        RequesterEmail = form["requesterEmail"].ToString(),
        Title = form["title"].ToString(),
        Category = form["category"].ToString(),
        Priority = Enum.Parse<TicketPriority>(form["priority"].ToString()),
        Description = form["description"].ToString(),
        Status = TicketStatus.Open
    };

    tickets.Create(ticket);

    return Results.Redirect($"/tickets/{ticket.Id}");
});

// Ticket details route: shows one ticket, its status form, and its work notes.
app.MapGet("/tickets/{id:int}", (int id, TicketService tickets) =>
{
    var ticket = tickets.GetById(id);

    if (ticket is null)
    {
        return Results.NotFound("Ticket not found.");
    }

    var comments = string.Join("", ticket.Comments
        .OrderByDescending(c => c.CreatedAt)
        .Select(c => $"""
        <div class="comment">
            <strong>{System.Net.WebUtility.HtmlEncode(c.Author)}</strong>
            <span>{c.CreatedAt:yyyy-MM-dd HH:mm}</span>
            <p>{System.Net.WebUtility.HtmlEncode(c.Message)}</p>
        </div>
        """));

    var statusOptions = string.Join("", Enum.GetNames<TicketStatus>().Select(status =>
        $"<option value='{status}' {(ticket.Status.ToString() == status ? "selected" : "")}>{status}</option>"));

    var html = Html.Layout($"Ticket #{ticket.Id}", $"""
        <section class="panel">
            <div class="ticket-header">
                <div>
                    <h1>#{ticket.Id} - {System.Net.WebUtility.HtmlEncode(ticket.Title)}</h1>
                    <p>{System.Net.WebUtility.HtmlEncode(ticket.Description)}</p>
                </div>
                <a class="button secondary" href="/">Back to Dashboard</a>
            </div>

            <div class="ticket-meta">
                <span><strong>Requester:</strong> {System.Net.WebUtility.HtmlEncode(ticket.RequesterName)}</span>
                <span><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(ticket.RequesterEmail)}</span>
                <span><strong>Category:</strong> {ticket.Category}</span>
                <span><strong>Priority:</strong> {ticket.Priority}</span>
                <span><strong>Status:</strong> {ticket.Status}</span>
                <span><strong>Created:</strong> {ticket.CreatedAt:yyyy-MM-dd HH:mm}</span>
            </div>
        </section>

        <section class="panel narrow">
            <h2>Update Status</h2>
            <form method="post" action="/tickets/{ticket.Id}/status">
                <label>Status</label>
                <select name="status">
                    {statusOptions}
                </select>
                <button class="button" type="submit">Update Status</button>
            </form>
        </section>

        <section class="panel narrow">
            <h2>Add Comment</h2>
            <form method="post" action="/tickets/{ticket.Id}/comments">
                <label>Your Name</label>
                <input name="author" required placeholder="Example: Matthew Smith">

                <label>Comment</label>
                <textarea name="message" required placeholder="Add troubleshooting notes or a resolution update..."></textarea>

                <button class="button" type="submit">Add Comment</button>
            </form>
        </section>

        <section class="panel">
            <h2>Comments / Work Notes</h2>
            {(string.IsNullOrWhiteSpace(comments) ? "<p>No comments yet.</p>" : comments)}
        </section>
    """);

    return Results.Content(html, "text/html");
});

// Updates the ticket status, for example from Open to InProgress or Closed.
app.MapPost("/tickets/{id:int}/status", async (int id, HttpRequest request, TicketService tickets) =>
{
    var form = await request.ReadFormAsync();
    var status = Enum.Parse<TicketStatus>(form["status"].ToString());

    tickets.UpdateStatus(id, status);

    return Results.Redirect($"/tickets/{id}");
});

// Adds a troubleshooting note/comment to a ticket.
app.MapPost("/tickets/{id:int}/comments", async (int id, HttpRequest request, TicketService tickets) =>
{
    var form = await request.ReadFormAsync();

    tickets.AddComment(id, new TicketComment
    {
        Author = form["author"].ToString(),
        Message = form["message"].ToString()
    });

    return Results.Redirect($"/tickets/{id}");
});

app.Run();

// Small helper used to keep the same page layout on every route.
static class Html
{
    public static string Layout(string title, string body)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{title} - Help Desk</title>
            <link rel="stylesheet" href="/css/site.css">
        </head>
        <body>
            <header class="topbar">
                <a href="/" class="brand">Help Desk</a>
                <nav>
                    <a href="/">Dashboard</a>
                    <a href="/tickets/new">New Ticket</a>
                </nav>
            </header>
            <main class="container">
                {body}
            </main>
            <footer>
                Built as an IT support portfolio project.
            </footer>
        </body>
        </html>
        """;
    }
}
