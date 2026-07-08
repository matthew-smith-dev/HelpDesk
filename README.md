# Help Desk

**A simple help desk ticketing system built with C# and ASP.NET Core.**

This is a student portfolio project. The goal is to show that I understand how a basic IT support ticketing system works and that I can build a small working web application from start to finish.

> Status: MVP Build

---

## What the App Does

The app lets a support team:

- view a dashboard
- create a support ticket
- open a ticket and read the details
- update the ticket status
- add technician notes/comments
- see ticket priority and category

---

## Tech Stack

- C#
- .NET 8
- ASP.NET Core
- HTML
- CSS
- JSON file storage

I used JSON storage for this first version so the project is easy to run without needing SQL Server. A future version can use SQLite or SQL Server.

---

## How to Run

Make sure the .NET 8 SDK is installed.

Open a terminal in the project folder and run:

```bash
dotnet run
```

Then open the localhost link shown in the terminal.

---

## Project Structure

```text
HelpDesk/
├── Models/
│   ├── Ticket.cs
│   ├── TicketComment.cs
│   ├── TicketPriority.cs
│   └── TicketStatus.cs
├── Services/
│   └── TicketService.cs
├── wwwroot/
│   └── css/
│       └── site.css
├── docs/
│   ├── roadmap.md
│   └── interview-notes.md
├── screenshots/
├── Program.cs
├── HelpDesk.csproj
└── README.md
```

---

## What I Learned

- how to create a simple ASP.NET Core web app
- how to build routes for pages and forms
- how to use C# models for tickets and comments
- how to save and load data from a JSON file
- how ticket status, priority, and work notes can be used in a help desk workflow

---

## Future Improvements

- add search and filters
- add technician assignment
- replace JSON storage with SQLite
- add user login
- add screenshots
- add basic automated tests
