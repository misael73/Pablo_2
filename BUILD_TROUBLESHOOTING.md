# Build Troubleshooting Guide

This guide helps resolve common build errors when working with the Sirefi .NET application.

---

## Error: NETSDK1004 - Assets file not found

### Error Message
```
Error NETSDK1004 : Assets file '/path/to/BlazorApp1/obj/project.assets.json' not found. 
Run a NuGet package restore to generate this file.
```

### Root Cause
NuGet packages have not been restored. This file is generated when NuGet packages are restored and is required for building the project.

### Solution

#### Option 1: Command Line (Recommended)
```bash
# Navigate to the BlazorApp1 directory
cd BlazorApp1

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build
```

#### Option 2: Using Visual Studio / Rider
1. Right-click on the solution/project in Solution Explorer
2. Select "Restore NuGet Packages"
3. Wait for restoration to complete
4. Build the project (Ctrl+Shift+B or Cmd+Shift+B)

#### Option 3: Clean and Restore
If the above doesn't work, try a clean restore:
```bash
cd BlazorApp1

# Clean the project
dotnet clean

# Restore packages
dotnet restore

# Build
dotnet build
```

### Prevention
Always run `dotnet restore` when:
- First cloning the repository
- After pulling new changes that modify dependencies
- After cleaning the project
- After switching branches

---

## Error: Build Failed - General Troubleshooting

### 1. Check .NET SDK Version
Ensure you have .NET 9.0 SDK installed:
```bash
dotnet --version
```

Should show version 9.0.x or higher.

**Install .NET 9.0 SDK**:
- Windows/Mac/Linux: https://dotnet.microsoft.com/download/dotnet/9.0

### 2. Clean Build
```bash
# For Backend
cd Sirefi
dotnet clean
dotnet restore
dotnet build

# For Frontend
cd BlazorApp1
dotnet clean
dotnet restore
dotnet build
```

### 3. Clear NuGet Cache
If packages are corrupted:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
cd BlazorApp1
dotnet restore

cd ../Sirefi
dotnet restore
```

### 4. Check File Permissions
Ensure you have write permissions to the project directories:
```bash
# Linux/Mac
ls -la BlazorApp1/obj
ls -la Sirefi/obj

# If needed, fix permissions
chmod -R u+w BlazorApp1
chmod -R u+w Sirefi
```

---

## Error: Port Already in Use

### Error Message
```
Failed to bind to address https://localhost:7070: address already in use
```

### Solution

#### Option 1: Kill Existing Process
```bash
# Linux/Mac
lsof -ti:7070 | xargs kill -9
lsof -ti:5107 | xargs kill -9

# Windows (PowerShell)
Get-Process -Id (Get-NetTCPConnection -LocalPort 7070).OwningProcess | Stop-Process
```

#### Option 2: Use Different Port
Edit `BlazorApp1/Properties/launchSettings.json` to use different ports.

---

## Error: Connection Refused / Backend Not Accessible

### Symptoms
```
Failed to load resource: net::ERR_CONNECTION_REFUSED
:5201/api/auth/me:1
```

### Solution
1. **Ensure backend is running**:
   ```bash
   cd Sirefi
   dotnet run
   ```
   Should show: `Now listening on: http://localhost:5201`

2. **Check frontend configuration**:
   File: `BlazorApp1/wwwroot/appsettings.Development.json`
   ```json
   {
     "ApiBaseUrl": "http://localhost:5201"
   }
   ```

3. **Start in correct order**:
   - Start backend first: `cd Sirefi && dotnet run`
   - Then start frontend: `cd BlazorApp1 && dotnet run`

---

## Error: Database Connection Issues

### Error Message
```
Unable to connect to SQL Server
Login failed for user
```

### Solution
1. **Check connection string**:
   File: `Sirefi/appsettings.Development.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;..."
     }
   }
   ```

2. **Test SQL Server connection**:
   ```bash
   # Verify SQL Server is running
   # Update connection string with your credentials
   ```

3. **Run migrations** (if using EF Core):
   ```bash
   cd Sirefi
   dotnet ef database update
   ```

---

## Error: CORS Policy Errors

### Error Message
```
Access to fetch at 'http://localhost:5201/api/...' has been blocked by CORS policy
```

### Solution
Already configured in `Sirefi/Program.cs`. If issues persist:

1. **Verify CORS configuration**:
   ```csharp
   // In development, should have:
   policy.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader();
   ```

2. **Check frontend URL** matches allowed origins in production

3. **Clear browser cache** and restart both applications

---

## Error: Google Sign-In Not Working

### Symptoms
- Button doesn't appear
- Authentication fails
- Redirect doesn't work

### Solution
1. **Check Google Client ID**:
   File: `BlazorApp1/wwwroot/index.html`
   ```html
   <meta name="google-signin-client_id" content="YOUR_CLIENT_ID.apps.googleusercontent.com">
   ```

2. **Verify backend is reachable**:
   - Backend must be running on http://localhost:5201
   - Test: Open http://localhost:5201/api/auth/me in browser

3. **Check browser console** for specific errors

---

## Common Setup Issues

### Issue: "Command 'dotnet' not found"

**Solution**: Install .NET SDK
```bash
# Verify installation
dotnet --version

# If not installed, download from:
# https://dotnet.microsoft.com/download
```

### Issue: "Project file is corrupt"

**Solution**: Check .csproj files
```bash
# Validate project files
dotnet build-server shutdown
dotnet clean
dotnet restore
```

### Issue: Slow build times

**Solution**: Optimize build
```bash
# Build without restore (after first restore)
dotnet build --no-restore

# Skip tests during build
dotnet build --no-restore -c Release
```

---

## Quick Setup Checklist

Before reporting issues, verify:

- [ ] .NET 9.0 SDK installed (`dotnet --version`)
- [ ] NuGet packages restored (`dotnet restore`)
- [ ] Backend builds successfully (`cd Sirefi && dotnet build`)
- [ ] Frontend builds successfully (`cd BlazorApp1 && dotnet build`)
- [ ] Backend running (`cd Sirefi && dotnet run`)
- [ ] Frontend running (`cd BlazorApp1 && dotnet run`)
- [ ] Configuration files have correct URLs
- [ ] SQL Server accessible (if using database)
- [ ] Google Client ID configured (if using OAuth)

---

## Getting Help

If issues persist after trying these solutions:

1. **Check Documentation**:
   - `RUNNING_THE_APP.md` - Quick start guide
   - `QUICK_FIX_GUIDE.md` - Common issues
   - `FIXES_SUMMARY.md` - All fixes applied

2. **Verify Environment**:
   ```bash
   # Check .NET version
   dotnet --version
   
   # Check installed workloads
   dotnet workload list
   
   # Check project info
   dotnet --info
   ```

3. **Clean Everything**:
   ```bash
   # Nuclear option - clean everything
   find . -type d -name "bin" -exec rm -rf {} +
   find . -type d -name "obj" -exec rm -rf {} +
   dotnet nuget locals all --clear
   dotnet restore
   dotnet build
   ```

4. **Check Logs**:
   - Browser console (F12)
   - Backend console output
   - System logs

---

## Success Verification

After fixing build issues, verify everything works:

```bash
# 1. Backend builds and runs
cd Sirefi
dotnet restore
dotnet build
dotnet run
# Should show: "Now listening on: http://localhost:5201"

# 2. Frontend builds and runs (new terminal)
cd BlazorApp1
dotnet restore
dotnet build
dotnet run
# Should show: "Now listening on: https://localhost:7070"

# 3. Open browser
# Navigate to: https://localhost:7070
# Should see login page with Google Sign-In button
```

---

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [NuGet Documentation](https://docs.microsoft.com/nuget/)
- Project Documentation: See `COMPLETE_MIGRATION_SUMMARY.md`
