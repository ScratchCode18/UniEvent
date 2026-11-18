# UniEvent 🎓
## University Event Management System

UniEvent is a university event management system developed to simplify event organization, student registration, and administrative event management.

The project is developed in two deliverables, progressing from a static frontend to a functional ASP.NET Core MVC web application.

## Project Deliverables

### Deliverable 1 — Static Frontend
A responsive frontend built using:
- HTML5
- CSS3
- Bootstrap 5
- JavaScript

Includes the homepage, login, registration, student dashboard, organizer dashboard, and admin dashboard.

### Deliverable 2 — ASP.NET Core MVC Application
A functional web application implementing authentication, database integration, role-based access, and event management.

## User Roles and Features

### Student
- Register and log in securely.
- View upcoming approved university events.
- Register for events.
- View personal event registrations.
- Prevent duplicate event registrations.

### Event Organizer
- Access an organizer dashboard.
- Create new events.
- Edit and delete their own events.
- Submit events for administrative approval.
- View event statuses.

### Administrator
- Access the admin dashboard.
- View application statistics.
- Approve or reject submitted events.
- Manage registered users.
- Promote students to organizers.
- Activate and deactivate user accounts.

## Technology Stack

| Component | Technology |
|---|---|
| Frontend | HTML, CSS, Bootstrap 5, JavaScript |
| Backend | C#, ASP.NET Core MVC |
| Database | SQL Server LocalDB |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Core Identity |
| Development Environment | Visual Studio Code |

## Project Structure

```text
UniEvent/
├── Deliverable1-Static/
│   ├── css/
│   ├── index.html
│   ├── login.html
│   ├── signup.html
│   ├── student.html
│   ├── organizer.html
│   └── admin.html
│
├── UniEvent/
│   ├── Controllers/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── UniEvent.csproj
│
├── UniEvent.slnx
└── README.md
```

## Running Deliverable 2

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB
- Visual Studio Code or another compatible code editor

### Setup

1. Clone or download the repository.
2. Open a terminal in the project root.
3. Configure the database connection string in `UniEvent/appsettings.json`.
4. Configure the administrator seed credentials using .NET User Secrets:

```powershell
dotnet user-secrets init --project .\UniEvent\UniEvent.csproj

dotnet user-secrets set "AdminSeed:Email" "admin@unievent.edu" --project .\UniEvent\UniEvent.csproj

dotnet user-secrets set "AdminSeed:Password" "YourStrongPassword123" --project .\UniEvent\UniEvent.csproj

dotnet user-secrets set "AdminSeed:FullName" "System Administrator" --project .\UniEvent\UniEvent.csproj
```

5. Restore dependencies and run the application:

```powershell
dotnet restore .\UniEvent\UniEvent.csproj
dotnet run --project .\UniEvent\UniEvent.csproj
```

The application applies database migrations and seeds initial roles, users, and demonstration events during startup.

Open the local URL displayed in the terminal.

## Demo Accounts

The development database includes demonstration accounts:

| Role | Email | Password |
|---|---|---|
| Student | student@unievent.edu | Student@123! |
| Organizer | organizer@unievent.edu | Organizer@123! |
| Admin | admin@unievent.edu | Configured through User Secrets |

Demo account availability depends on the development database and seed configuration. The administrator password is configured separately and is not stored in this repository.

## Current Status

- Deliverable 1: Static frontend implementation completed.
- Deliverable 2: ASP.NET Core MVC implementation completed, including authentication, role-based dashboards, event management, database integration, and administrative controls.

UniEvent is an academic semester project developed for learning and demonstrating full-stack web application development.