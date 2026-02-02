# Running SIREFI - Development Guide

## Quick Start

### 1. Start the Backend API (Sirefi)

```bash
cd Sirefi
dotnet restore
dotnet run
```

The API will start on:
- **HTTPS**: https://localhost:7186
- **HTTP**: http://localhost:5201

### 2. Start the Frontend (BlazorApp1)

In a new terminal:

```bash
cd BlazorApp1
dotnet restore
dotnet run
```

The Blazor app will start on:
- **HTTPS**: https://localhost:7070
- **HTTP**: http://localhost:5107

### 3. Access the Application

Open your browser to:
- **Blazor App**: https://localhost:7070
- **API Swagger** (if configured): https://localhost:7186/swagger

---

## Configuration

### Backend API (Sirefi)

**Location**: `Sirefi/appsettings.Development.json`

Key settings:
- `ConnectionStrings:DefaultConnection` - Database connection
- `Google:ClientId` - Google OAuth client ID
- `AllowedOrigins` - CORS allowed origins (includes Blazor app URLs)

**Ports** (from `Sirefi/Properties/launchSettings.json`):
- HTTPS: 7186
- HTTP: 5201

### Frontend (BlazorApp1)

**Location**: `BlazorApp1/wwwroot/appsettings.Development.json`

Key settings:
- `ApiBaseUrl` - Backend API URL (should be `https://localhost:7186`)

**Ports** (from `BlazorApp1/Properties/launchSettings.json`):
- HTTPS: 7070
- HTTP: 5107

---

## Troubleshooting

### Error: ERR_CONNECTION_REFUSED

**Symptom**: Blazor app shows connection refused when calling API

**Solution**: Make sure the backend API (Sirefi) is running before starting the Blazor app.

### Error: CORS Policy Error

**Symptom**: Browser console shows CORS policy error

**Solution**: Verify that `Sirefi/appsettings.Development.json` includes the Blazor app URLs in `AllowedOrigins`:
```json
"AllowedOrigins": [
  "http://localhost:5107",
  "https://localhost:7070"
]
```

### Error: Login Fails with Google

**Symptom**: Google OAuth login doesn't work

**Solution**: 
1. Verify `Google:ClientId` in `Sirefi/appsettings.Development.json`
2. Make sure the Google OAuth app has the correct redirect URIs configured
3. Check browser console for detailed error messages

---

## Development Workflow

1. **Make Backend Changes**
   - Edit files in `Sirefi/`
   - The API will auto-reload (if using `dotnet watch run`)

2. **Make Frontend Changes**
   - Edit files in `BlazorApp1/`
   - The Blazor app will auto-reload (if using `dotnet watch run`)

3. **Database Changes**
   - Update models in `Sirefi/Models/`
   - Run migrations if needed
   - Restart the API

---

## Port Summary

| Application | Protocol | Port | URL |
|-------------|----------|------|-----|
| Backend API | HTTPS | 7186 | https://localhost:7186 |
| Backend API | HTTP | 5201 | http://localhost:5201 |
| Blazor App | HTTPS | 7070 | https://localhost:7070 |
| Blazor App | HTTP | 5107 | http://localhost:5107 |

---

## Production Deployment

For production deployment:
1. Update `BlazorApp1/wwwroot/appsettings.json` with production API URL
2. Update `Sirefi/appsettings.json` with production settings
3. Build both projects in Release mode:
   ```bash
   dotnet publish -c Release
   ```
4. Deploy backend to your API hosting service
5. Deploy frontend to static web hosting (Azure Static Web Apps, Netlify, etc.)
