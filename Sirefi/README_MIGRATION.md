# SIREFI - .NET Migration

This is the .NET (C# ASP.NET Core) migration of the original PHP SIREFI application from the `pablo` folder.

## Project Structure

```
Sirefi/
├── Controllers/          # API Controllers
│   ├── AuthController.cs           # Authentication endpoints
│   ├── ReportesController.cs       # Report management
│   ├── EdificiosController.cs      # Buildings management
│   ├── SalonesController.cs        # Rooms management
│   └── CategoriasController.cs     # Categories management
├── Services/             # Business logic layer
│   ├── AuthService.cs              # Google OAuth authentication
│   ├── UserService.cs              # User management
│   ├── ReporteService.cs           # Report operations
│   ├── FileService.cs              # File upload/download
│   ├── InfrastructureService.cs    # Buildings and rooms
│   └── CategoryService.cs          # Categories
├── DTOs/                 # Data Transfer Objects
├── Models/               # Entity Framework models
├── Data/                 # DbContext
└── Program.cs            # Application configuration
```

## Features Migrated

✅ **Authentication**
- Google OAuth 2.0 integration
- Cookie-based authentication
- Role-based authorization (Administrador, Reportante, Técnico)

✅ **Report Management**
- Create, read, update, delete reports
- File attachments
- Status tracking
- Priority levels
- Folio generation

✅ **Infrastructure Management**
- Buildings (Edificios) CRUD
- Rooms (Salones) CRUD
- Active/inactive status

✅ **Category Management**
- Categories CRUD
- Dashboard type classification
- Active/inactive toggling

✅ **Dashboards**
- General statistics
- Filtered by dashboard type (Materiales, TICs, etc.)

## Database

The application uses the existing SIREFI SQL Server database. Entity Framework Core is configured with the database-first approach.

Connection string is configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SIREFI;User Id=sa;Password=Alucard12#;TrustServerCertificate=True;"
  }
}
```

## API Endpoints

### Authentication
- `POST /api/auth/google` - Validate Google OAuth token
- `POST /api/auth/logout` - Logout
- `GET /api/auth/me` - Get current user

### Reports
- `GET /api/reportes` - Get all reports (filtered by role)
- `GET /api/reportes/{id}` - Get report by ID
- `GET /api/reportes/folio/{folio}` - Get report by folio
- `POST /api/reportes` - Create new report
- `PUT /api/reportes/{id}` - Update report (Admin/Technician only)
- `DELETE /api/reportes/{id}` - Delete report (Admin only)
- `POST /api/reportes/{id}/upload` - Upload file to report
- `GET /api/reportes/stats` - Get dashboard statistics

### Buildings
- `GET /api/edificios` - Get all buildings
- `GET /api/edificios/{id}` - Get building by ID
- `POST /api/edificios` - Create building (Admin only)
- `PUT /api/edificios/{id}` - Update building (Admin only)
- `DELETE /api/edificios/{id}` - Delete building (Admin only)

### Rooms
- `GET /api/salones?edificio_id={id}` - Get rooms by building
- `GET /api/salones/{id}` - Get room by ID
- `POST /api/salones` - Create room (Admin only)
- `PUT /api/salones/{id}` - Update room (Admin only)
- `DELETE /api/salones/{id}` - Delete room (Admin only)

### Categories
- `GET /api/categorias` - Get active categories
- `GET /api/categorias/all` - Get all categories (Admin only)
- `GET /api/categorias/{id}` - Get category by ID
- `POST /api/categorias` - Create category (Admin only)
- `PUT /api/categorias/{id}` - Update category (Admin only)
- `PUT /api/categorias/{id}/toggle` - Toggle active status (Admin only)
- `DELETE /api/categorias/{id}` - Delete category (Admin only)

## Running the Application

### Prerequisites
- .NET 9.0 SDK
- SQL Server with SIREFI database

### Setup
1. Ensure SQL Server is running and SIREFI database exists
2. Update connection string in `appsettings.json` if needed
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

### Run
```bash
cd Sirefi
dotnet run
```

The application will start on:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

### Development
For development with auto-reload:
```bash
dotnet watch run
```

## Configuration

### File Upload
Configure file upload settings in `appsettings.json`:
```json
{
  "FileUpload": {
    "UploadPath": "uploads",
    "MaxFileSize": 2097152,
    "AllowedExtensions": ["jpg", "jpeg", "png", "pdf"]
  }
}
```

### Google OAuth
Configure Google OAuth Client ID in `appsettings.json`:
```json
{
  "Google": {
    "ClientId": "your-google-client-id"
  }
}
```

## Security Features

- **Authentication**: Cookie-based authentication with Google OAuth
- **Authorization**: Role-based access control (Admin, Reportante, Técnico)
- **Input Validation**: DTO validation and sanitization
- **File Upload Validation**: File type and size restrictions
- **SQL Injection Protection**: Entity Framework parameterized queries
- **Session Management**: Secure session configuration

## Differences from PHP Version

1. **Architecture**: 
   - PHP: Procedural with some OOP
   - .NET: Clean architecture with services, DTOs, and controllers

2. **Authentication**:
   - PHP: Session-based with manual session handling
   - .NET: Cookie-based authentication with ASP.NET Core Identity claims

3. **Database Access**:
   - PHP: sqlsrv extension with raw queries
   - .NET: Entity Framework Core ORM

4. **File Handling**:
   - PHP: Direct file operations
   - .NET: IFormFile interface with streaming

5. **API Structure**:
   - PHP: Multiple PHP files with mixed concerns
   - .NET: RESTful API with dedicated controllers

## Next Steps

To complete the migration:

1. **Frontend Migration**: The PHP views need to be migrated to:
   - Blazor (recommended for .NET integration)
   - React/Vue.js with API consumption
   - Or keep as static HTML/JS calling the .NET API

2. **Testing**: Add unit tests and integration tests

3. **Deployment**: Configure for production deployment
   - Azure App Service
   - IIS
   - Docker containers

4. **Additional Features**:
   - Email notifications
   - PDF export
   - Real-time updates with SignalR
   - Advanced reporting

## Support

For issues or questions about this migration, refer to the original PHP documentation in the `pablo` folder.
