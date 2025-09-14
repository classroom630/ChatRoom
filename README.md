# ChatRoom Task Management System

A comprehensive User & Task Management System built with ASP.NET Core, featuring JWT authentication, role-based authorization, and multi-layered architecture.

![Home Page](https://github.com/user-attachments/assets/15c4d8e5-6de6-4c58-87b1-6be50957a49b)

## Features

- **JWT Authentication** with refresh tokens
- **Role-based Authorization** (Admin, Manager, User)
- **Multi-layered Architecture** (Repository + Service + Controller)
- **RESTful Web API** with comprehensive endpoints
- **MVC Frontend** with role-based UI
- **Entity Framework Core** with SQLite database
- **Structured Logging** with Serilog
- **Email Integration** for welcome messages
- **Unit Testing** with xUnit and Moq

## Architecture

### Project Structure
```
ChatRoom/
├── ChatRoom.API/          # Web API project
├── ChatRoom.MVC/          # MVC frontend project  
├── ChatRoom.Models/       # Shared models, DTOs, ViewModels
├── ChatRoom.Tests/        # Unit tests
└── ChatRoom.sln          # Solution file
```

### Technology Stack
- **Backend**: ASP.NET Core 8.0 Web API
- **Frontend**: ASP.NET Core 8.0 MVC with Razor Views
- **Database**: SQLite with Entity Framework Core
- **Authentication**: JWT with ASP.NET Core Identity
- **Logging**: Serilog
- **Testing**: xUnit with Moq

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ChatRoom
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run database migrations** (from ChatRoom.API directory)
   ```bash
   cd ChatRoom.API
   dotnet ef database update
   ```

5. **Start the API server**
   ```bash
   cd ChatRoom.API
   dotnet run --urls="http://localhost:5151"
   ```

6. **Start the MVC application** (in another terminal)
   ```bash
   cd ChatRoom.MVC  
   dotnet run --urls="http://localhost:5000"
   ```

7. **Access the application**
   - MVC Frontend: http://localhost:5000
   - API Endpoints: http://localhost:5151
   - Swagger UI: http://localhost:5151/swagger

### Demo Accounts

The system comes with pre-seeded demo accounts:

| Role | Email | Password | Permissions |
|------|-------|----------|-------------|
| **Admin** | admin@chatroom.com | Admin123! | Full system access, user management |
| **Manager** | manager@chatroom.com | Manager123! | Team task management |
| **User** | user@chatroom.com | User123! | Personal task management |

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login + JWT + refresh token  
- `POST /api/auth/refresh` - Refresh JWT using refresh token
- `POST /api/auth/logout` - Logout and revoke tokens

### Users (Admin only)
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/assign-role` - Assign role to user

### Tasks
- `GET /api/tasks` - Get tasks (filtered by role)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task  
- `DELETE /api/tasks/{id}` - Delete task
- `PUT /api/tasks/{id}/toggle` - Toggle task completion

## Role-Based Access Control

### Admin Dashboard
![Admin Dashboard](https://github.com/user-attachments/assets/2263308b-e82d-4ace-89d2-86aceda1b615)

**Admin permissions:**
- Full system access
- User management (create, edit, delete, assign roles)
- View all tasks across the system
- System statistics and analytics

### User Management (Admin Only)
![User Management](https://github.com/user-attachments/assets/33bd139b-bd3f-4f6f-bd09-4823d4dd753d)

### User Dashboard
![User Dashboard](https://github.com/user-attachments/assets/bcd96c03-9e27-4cd4-af05-d291485b4ad3)

**User permissions:**
- Personal task management
- View own task statistics
- Cannot access user management

### Manager Dashboard
**Manager permissions:**
- Team task management
- View team member tasks
- Cannot manage users

## Testing

Run the unit tests:
```bash
cd ChatRoom.Tests
dotnet test
```

The test suite includes:
- **AuthService Tests**: Login, registration, token management
- **UserService Tests**: User CRUD operations, role assignment
- **TaskService Tests**: Task management with authorization

## Configuration

### Database Connection
Configure the database connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=chatroom.db"
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "ChatRoom.API",
    "Audience": "ChatRoom.Client", 
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Email Settings (Optional)
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@chatroom.com",
    "FromName": "ChatRoom System"
  }
}
```

## Development

### Adding New Features
1. Add models to `ChatRoom.Models`
2. Implement services in `ChatRoom.API/Services`
3. Create controllers in `ChatRoom.API/Controllers`
4. Add MVC views and controllers in `ChatRoom.MVC`
5. Write unit tests in `ChatRoom.Tests`

### Database Migrations
```bash
cd ChatRoom.API
dotnet ef migrations add MigrationName
dotnet ef database update
```

## License

This project is licensed under the MIT License.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## Support

For issues and questions, please open an issue on the GitHub repository.