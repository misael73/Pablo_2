# Quick Fix Guide - Common Errors

## 🚨 Quick Error Reference

### Build Errors - NETSDK1004

```
ERROR: Assets file 'obj/project.assets.json' not found. 
Run a NuGet package restore to generate this file.
```

**Quick Fix**:
```bash
cd BlazorApp1  # or cd Sirefi
dotnet restore
dotnet build
```

**See**: `BUILD_TROUBLESHOOTING.md` for complete guide

---

### Connection Refused Errors

```
ERROR: Failed to load resource: net::ERR_CONNECTION_REFUSED
```

**Quick Fix**:
1. ✅ Backend must be running: `cd Sirefi && dotnet run`
2. ✅ Check port 7186 is listening
3. ✅ Frontend configured to use https://localhost:7186

**See**: `FIX_CONNECTION_ERROR.md` for details

---

### CORS Errors

```
ERROR: Access to XMLHttpRequest has been blocked by CORS policy
```

**Quick Fix**:
- ✅ CORS now allows any origin in development
- ✅ Restart backend after changes
- ✅ Clear browser cache

**See**: `FIX_CORS_AND_BLAZOR_ERRORS.md` for details

---

### Authorization Errors

```
ERROR: Authorization failed. DenyAnonymousAuthorizationRequirement
```

**Quick Fix**:
- ✅ Login page has `[AllowAnonymous]` attribute
- ✅ Clear browser cookies
- ✅ Try incognito/private mode

**See**: `FIX_CORS_AND_BLAZOR_ERRORS.md` for details

---

### Blazor Component Errors

```
ERROR: There is already a subscriber to the content with the given section ID
```

**Quick Fix**:
- ✅ HeadOutlet only in index.html (not in layouts)
- ✅ Layouts should not have HTML/HEAD tags
- ✅ Already fixed in MinimalLayout.razor

**See**: `FIX_CORS_AND_BLAZOR_ERRORS.md` for details

---

## 🚀 Quick Start Commands

### Start Backend
```bash
cd Sirefi
dotnet run
```
Expected: `Now listening on: https://localhost:7186`

### Start Frontend
```bash
cd BlazorApp1
dotnet run
```
Expected: `Now listening on: https://localhost:7070`

### Open Application
```
Browser: https://localhost:7070
```

---

## ✅ Quick Health Check

### 1. Backend Running?
```bash
curl -k https://localhost:7186/api/auth/me
# Should get response (even if 401 Unauthorized)
```

### 2. Frontend Running?
```bash
curl http://localhost:7070
# Should get HTML response
```

### 3. CORS Working?
Open browser console (F12) → Network tab
- Check API calls to :7186
- Should NOT see CORS errors
- Should see 200 or 401 responses

### 4. Login Page Working?
- Navigate to https://localhost:7070
- Should redirect to /login
- Google Sign-In button should appear
- No errors in console

---

## 🔍 Debug Checklist

If things aren't working:

- [ ] Backend is running (check terminal)
- [ ] Frontend is running (check terminal)
- [ ] Using correct ports (7186 backend, 7070 frontend)
- [ ] Browser cache cleared
- [ ] No other apps using those ports
- [ ] Check browser console for errors
- [ ] Try incognito/private mode

---

## 📚 Detailed Documentation

For more details, see:

| Issue | Document |
|-------|----------|
| How to run | `RUNNING_THE_APP.md` |
| Build errors | `BUILD_TROUBLESHOOTING.md` ← NEW |
| Port configuration | `FIX_CONNECTION_ERROR.md` |
| CORS & Blazor errors | `FIX_CORS_AND_BLAZOR_ERRORS.md` |
| System architecture | `CONNECTION_ARCHITECTURE.md` |
| Frontend migration | `FRONTEND_MIGRATION_SUMMARY.md` |
| Backend migration | `MIGRATION_SUMMARY.md` |
| Complete overview | `COMPLETE_MIGRATION_SUMMARY.md` |

---

## 🆘 Still Having Issues?

1. Read the relevant detailed documentation above
2. Check browser console (F12) for specific errors
3. Check terminal output for backend/frontend
4. Try cleaning and rebuilding:
   ```bash
   cd Sirefi
   dotnet clean
   dotnet build
   
   cd ../BlazorApp1
   dotnet clean
   dotnet build
   ```

---

**Quick Tip**: Most errors are fixed by:
1. Making sure both apps are running
2. Clearing browser cache
3. Using the correct ports
