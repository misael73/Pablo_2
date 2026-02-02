# Connection Architecture - SIREFI

## Overview Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         Browser                              │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   Blazor WebAssembly App                              │  │
│  │   URL: https://localhost:7070                         │  │
│  │                                                        │  │
│  │   ┌──────────────────────────────────────────────┐   │  │
│  │   │  appsettings.Development.json                │   │  │
│  │   │  {                                            │   │  │
│  │   │    "ApiBaseUrl": "https://localhost:7186"    │   │  │
│  │   │  }                                            │   │  │
│  │   └──────────────────────────────────────────────┘   │  │
│  │                       │                               │  │
│  │                       │ API Calls                     │  │
│  │                       ▼                               │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────┬───────────────────────────────┘
                              │
                              │ HTTPS Request
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Backend Server                            │
│  ┌───────────────────────────────────────────────────────┐  │
│  │   ASP.NET Core Web API (Sirefi)                       │  │
│  │   URL: https://localhost:7186                         │  │
│  │                                                        │  │
│  │   ┌──────────────────────────────────────────────┐   │  │
│  │   │  appsettings.Development.json                │   │  │
│  │   │  {                                            │   │  │
│  │   │    "AllowedOrigins": [                       │   │  │
│  │   │      "https://localhost:7070",  ← Blazor     │   │  │
│  │   │      "http://localhost:5107"    ← Blazor     │   │  │
│  │   │    ]                                          │   │  │
│  │   │  }                                            │   │  │
│  │   └──────────────────────────────────────────────┘   │  │
│  │                       │                               │  │
│  │                       ▼                               │  │
│  │   ┌──────────────────────────────────────────────┐   │  │
│  │   │  SQL Server Database (SIREFI)                │   │  │
│  │   └──────────────────────────────────────────────┘   │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Request Flow

### 1. Initial Page Load

```
User Browser
    │
    ├─> GET https://localhost:7070/
    │
    ▼
Blazor App
    │
    ├─> Loads WebAssembly
    ├─> Reads appsettings.Development.json
    └─> Initializes with ApiBaseUrl: "https://localhost:7186"
```

### 2. Authentication Check

```
Blazor App (AuthStateProvider)
    │
    ├─> GET https://localhost:7186/api/auth/me
    │   Headers:
    │   - Origin: https://localhost:7070
    │   - Cookie: (if exists)
    │
    ▼
Backend API (Sirefi)
    │
    ├─> CORS Check: Is origin allowed?
    │   ✅ YES: "https://localhost:7070" in AllowedOrigins
    │
    ├─> Authentication Check: Valid cookie?
    │   - YES: Return user info
    │   - NO: Return 401 Unauthorized
    │
    └─> Response sent back to Blazor
```

### 3. API Calls (e.g., Get Reports)

```
Blazor App
    │
    ├─> GET https://localhost:7186/api/reportes
    │   Headers:
    │   - Origin: https://localhost:7070
    │   - Cookie: .AspNetCore.Cookies=...
    │
    ▼
Backend API
    │
    ├─> CORS Check: ✅ Allowed
    ├─> Auth Check: ✅ Valid user
    ├─> Query Database
    │
    └─> Return JSON response
```

## Port Summary Table

| Service | Type | Port | URL | Purpose |
|---------|------|------|-----|---------|
| Blazor App | HTTPS | 7070 | https://localhost:7070 | Frontend UI |
| Blazor App | HTTP | 5107 | http://localhost:5107 | Frontend UI (HTTP) |
| Backend API | HTTPS | 7186 | https://localhost:7186 | REST API |
| Backend API | HTTP | 5201 | http://localhost:5201 | REST API (HTTP) |

## Configuration Mapping

### Frontend → Backend Connection

```
BlazorApp1/wwwroot/appsettings.Development.json
┌──────────────────────────────────────┐
│ "ApiBaseUrl": "https://localhost:7186" │ ──────┐
└──────────────────────────────────────┘       │
                                                │
                                                │ Points to
                                                │
                                                ▼
Sirefi/Properties/launchSettings.json
┌──────────────────────────────────────┐
│ "applicationUrl":                      │
│   "https://localhost:7186;             │ ◄─────┘
│    http://localhost:5201"              │
└──────────────────────────────────────┘
```

### Backend → Frontend CORS

```
BlazorApp1/Properties/launchSettings.json
┌──────────────────────────────────────┐
│ "applicationUrl":                      │
│   "https://localhost:7070;             │ ──────┐
│    http://localhost:5107"              │       │
└──────────────────────────────────────┘       │
                                                │ Must be in
                                                │
                                                ▼
Sirefi/appsettings.Development.json
┌──────────────────────────────────────┐
│ "AllowedOrigins": [                    │
│   "https://localhost:7070",            │ ◄─────┤
│   "http://localhost:5107"              │ ◄─────┘
│ ]                                      │
└──────────────────────────────────────┘
```

## Error Scenarios

### ❌ Before Fix: ERR_CONNECTION_REFUSED

```
Blazor App
    │
    ├─> Trying to call: http://localhost:5000/api/auth/me
    │                    ▲
    │                    └─ WRONG PORT!
    │
    ▼
Nothing listening on port 5000
    │
    └─> ERROR: ERR_CONNECTION_REFUSED
```

### ✅ After Fix: Success

```
Blazor App
    │
    ├─> Calling: https://localhost:7186/api/auth/me
    │            ▲
    │            └─ CORRECT PORT!
    │
    ▼
Backend API running on port 7186
    │
    ├─> CORS: ✅ Origin allowed
    ├─> Process request
    │
    └─> SUCCESS: Return 200 OK with data
```

## CORS Flow Detail

### Request

```
Browser
    │
    ├─> Preflight (OPTIONS) Request
    │   URL: https://localhost:7186/api/auth/me
    │   Headers:
    │   - Origin: https://localhost:7070
    │   - Access-Control-Request-Method: GET
    │
    ▼
Backend API (Sirefi)
    │
    ├─> Check Program.cs CORS policy "AllowFrontend"
    │   - Load AllowedOrigins from appsettings
    │   - Check if "https://localhost:7070" is in list
    │
    ├─> Is origin allowed?
    │   ✅ YES: "https://localhost:7070" found
    │
    └─> Response:
        - Access-Control-Allow-Origin: https://localhost:7070
        - Access-Control-Allow-Methods: GET, POST, PUT, DELETE
        - Access-Control-Allow-Headers: *
        - Access-Control-Allow-Credentials: true
```

### Actual Request (after preflight)

```
Browser
    │
    ├─> GET Request
    │   URL: https://localhost:7186/api/auth/me
    │   Headers:
    │   - Origin: https://localhost:7070
    │   - Cookie: .AspNetCore.Cookies=...
    │
    ▼
Backend API
    │
    ├─> CORS already validated ✅
    ├─> Authenticate user from cookie
    ├─> Execute controller action
    │
    └─> Response with CORS headers + data
```

## Development Setup Checklist

### Terminal 1: Backend
```bash
cd Sirefi
dotnet run

Expected output:
✅ Now listening on: https://localhost:7186
✅ Now listening on: http://localhost:5201
```

### Terminal 2: Frontend
```bash
cd BlazorApp1
dotnet run

Expected output:
✅ Now listening on: https://localhost:7070
✅ Now listening on: http://localhost:5107
```

### Browser
```
Open: https://localhost:7070

Expected behavior:
✅ Page loads
✅ No connection errors in console
✅ Can login with Google
✅ Can view data
```

## Troubleshooting Decision Tree

```
Connection Error?
    │
    ├─ ERR_CONNECTION_REFUSED
    │   │
    │   └─ Is backend running?
    │       ├─ NO → Start backend: cd Sirefi && dotnet run
    │       └─ YES → Check ports in appsettings.Development.json
    │
    ├─ CORS Error
    │   │
    │   └─ Is frontend origin in AllowedOrigins?
    │       ├─ NO → Add to Sirefi/appsettings.Development.json
    │       └─ YES → Restart backend
    │
    └─ 401 Unauthorized
        │
        └─ Is user logged in?
            ├─ NO → Click "Sign in with Google"
            └─ YES → Check cookie in browser DevTools
```

## File Dependencies

```
Configuration Files:
    │
    ├─ BlazorApp1/wwwroot/appsettings.Development.json
    │   └─ ApiBaseUrl → Must match backend URL
    │
    ├─ Sirefi/appsettings.Development.json
    │   ├─ AllowedOrigins → Must include Blazor URLs
    │   ├─ ConnectionStrings → Database connection
    │   └─ Google:ClientId → OAuth configuration
    │
    ├─ Sirefi/Properties/launchSettings.json
    │   └─ applicationUrl → Backend ports
    │
    └─ BlazorApp1/Properties/launchSettings.json
        └─ applicationUrl → Frontend ports
```

---

**Last Updated**: After connection error fix
**Status**: ✅ Working correctly
