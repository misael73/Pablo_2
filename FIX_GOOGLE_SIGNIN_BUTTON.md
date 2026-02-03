# Fix: Google Sign-In Button Not Appearing

## Problem Description

The user reported that the Google Sign-In button was not appearing on the login page, with these error messages:

```
MONO_WASM: Version mismatch warnings (can be ignored - dotnet runtime cache issue)
:7186/api/auth/me:1  Failed to load resource: net::ERR_CONNECTION_REFUSED
Authorization failed. DenyAnonymousAuthorizationRequirement: Requires an authenticated user.
```

**Main Issue**: "no me aparece el boton de logeo" (the login button doesn't appear)

---

## Root Causes

### 1. JavaScript Interop Not Working
The `HandleGoogleLogin` method was declared as `static`, which meant:
- It couldn't access instance members like `AuthStateProvider` and `Navigation`
- It couldn't call the `LoginWithGoogle` method
- The token was received but never processed

### 2. Missing JavaScript Bridge
There was no proper connection between Google's OAuth callback and the Blazor component:
- Google called `handleCredentialResponse` 
- But there was no way to invoke the Blazor component's method
- The callback existed but did nothing useful

### 3. Authorization Issues
- Home page didn't have `[AllowAnonymous]` attribute
- When backend was down, auth checks failed and caused redirect loops
- No error handling for connection failures

---

## Solution Implemented

### 1. Fixed JavaScript Interop in Login.razor

**Before**:
```csharp
[JSInvokable]
public static async Task HandleGoogleLogin(string idToken)
{
    // Just logs, doesn't do anything
    Console.WriteLine($"Received token: {idToken}");
}
```

**After**:
```csharp
private DotNetObjectReference<Login>? objRef;

protected override void OnInitialized()
{
    objRef = DotNetObjectReference.Create(this);
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await JS.InvokeVoidAsync("setupGoogleSignIn", objRef);
    }
}

[JSInvokable]
public async Task HandleGoogleLogin(string idToken)
{
    // Now actually processes the token
    var result = await AuthStateProvider.Login(idToken);
    // Redirects on success, shows error on failure
}

public void Dispose()
{
    objRef?.Dispose();
}
```

**Key Changes**:
- ✅ Uses `DotNetObjectReference` to pass component instance to JavaScript
- ✅ Non-static method can access instance members
- ✅ Properly processes the Google token
- ✅ Implements `IDisposable` for cleanup
- ✅ Shows loading spinner during login
- ✅ Displays error messages to user

### 2. Added JavaScript Function in index.html

```javascript
window.setupGoogleSignIn = function (dotnetHelper) {
    console.log('Setting up Google Sign-In callback');
    window.loginComponentRef = dotnetHelper;
    
    window.handleCredentialResponse = function (response) {
        console.log('Google Sign-In response received');
        console.log('Token JWT recibido:', response.credential);
        
        if (window.loginComponentRef) {
            window.loginComponentRef.invokeMethodAsync('HandleGoogleLogin', response.credential)
                .then(() => console.log('Login method invoked successfully'))
                .catch(err => console.error('Error invoking login method:', err));
        } else {
            console.error('Login component reference not found');
        }
    };
};
```

**What This Does**:
1. Stores the Blazor component reference globally
2. Sets up `handleCredentialResponse` that Google OAuth calls
3. When Google returns a token, invokes the Blazor method
4. Includes error handling and logging

### 3. Fixed Home.razor Authorization

**Before**:
```csharp
@page "/"
@inject NavigationManager Navigation
@inject AuthenticationStateProvider AuthStateProvider

protected override async Task OnInitializedAsync()
{
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    // Could fail if backend is down
}
```

**After**:
```csharp
@page "/"
@attribute [Microsoft.AspNetCore.Authorization.AllowAnonymous]
@inject NavigationManager Navigation
@inject AuthenticationStateProvider AuthStateProvider

protected override async Task OnInitializedAsync()
{
    try
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        // ... redirect logic
    }
    catch
    {
        // If auth check fails, just go to login
        Navigation.NavigateTo("/login");
    }
}
```

**Key Changes**:
- ✅ Added `[AllowAnonymous]` attribute
- ✅ Added try-catch for backend connection errors
- ✅ Graceful fallback to login page

---

## How It Works Now

### Login Flow

1. **User navigates to /** → Home.razor
2. **Home checks auth** → Not authenticated or backend error
3. **Redirects to /login** → Login.razor loads
4. **OnAfterRenderAsync called** → `setupGoogleSignIn(objRef)` invoked
5. **JavaScript sets up callback** → Stores component reference
6. **Google button renders** → User sees "Sign in with Google"
7. **User clicks button** → Google OAuth popup opens
8. **User authenticates** → Google calls `handleCredentialResponse`
9. **JavaScript invokes Blazor** → `HandleGoogleLogin(token)` called
10. **Blazor validates token** → Calls backend API
11. **On success** → Redirects to dashboard or my-reports
12. **On failure** → Shows error message

### Component Lifecycle

```
Login.razor Lifecycle:
│
├─ OnInitialized()
│  └─ Creates DotNetObjectReference
│
├─ OnAfterRenderAsync(firstRender=true)
│  └─ Calls setupGoogleSignIn with reference
│
├─ [User clicks Google button]
│
├─ Google OAuth happens
│
├─ Google calls handleCredentialResponse
│
├─ JavaScript calls HandleGoogleLogin
│  └─ Shows loading spinner
│  └─ Calls AuthStateProvider.Login()
│  └─ Redirects or shows error
│
└─ Dispose()
   └─ Cleans up DotNetObjectReference
```

---

## Testing the Fix

### Prerequisites
- Backend (Sirefi) should be running on https://localhost:7186
- Frontend (BlazorApp1) should be running on https://localhost:7070

### Test Steps

1. **Start Backend**:
```bash
cd Sirefi
dotnet run
```

2. **Start Frontend**:
```bash
cd BlazorApp1
dotnet run
```

3. **Open Browser**: https://localhost:7070

4. **Expected Results**:
   - ✅ Redirects to /login
   - ✅ Google Sign-In button appears
   - ✅ No errors in browser console (except backend connection if not running)
   - ✅ Clicking button opens Google OAuth popup
   - ✅ After signing in with Google, redirects to dashboard
   - ✅ Console shows: "Token JWT recibido", "Login method invoked successfully"

### Test Without Backend

1. **Don't start backend**
2. **Start only frontend**: `cd BlazorApp1 && dotnet run`
3. **Open browser**: https://localhost:7070

**Expected**:
- ✅ Login page still loads
- ✅ Google Sign-In button still appears
- ✅ Can click button and get Google token
- ❌ Login will fail with error message (backend not available)
- ✅ Error message is displayed to user

---

## Common Issues & Solutions

### Button Still Not Appearing?

**Check Browser Console**:
```javascript
// Should see these messages:
Setting up Google Sign-In callback
Google Sign-In response received
Token JWT recibido: eyJhbGc...
Login method invoked successfully
```

**If Not Seeing Messages**:
1. Hard refresh (Ctrl+Shift+R or Cmd+Shift+R)
2. Clear browser cache
3. Check Google Identity Services script loaded: View Page Source → Look for `accounts.google.com/gsi/client`

### Login Button Appears But Doesn't Work?

**Check**:
1. Backend is running: `curl -k https://localhost:7186/api/auth/me`
2. CORS allows your origin (already configured with AllowAnyOrigin in dev)
3. Google Client ID is correct in Login.razor

### Error: "Login component reference not found"

This means `setupGoogleSignIn` wasn't called properly.

**Solution**:
1. Make sure `OnAfterRenderAsync` is being called
2. Check for JavaScript errors before this point
3. Try reloading the page

### Version Mismatch Warnings

```
MONO_WASM: The version of dotnet.runtime.js ... is different from dotnet.js
```

**These can be ignored**. They're caused by browser caching different versions of files. Solutions:
1. Hard refresh (Ctrl+Shift+R)
2. Clear browser cache
3. Close and reopen browser

---

## Architecture Decisions

### Why DotNetObjectReference?

**Alternative Approaches**:
1. ❌ Static methods with global app state
2. ❌ JavaScript-only solution with fetch calls
3. ✅ DotNetObjectReference with instance methods

**Benefits of DotNetObjectReference**:
- ✅ Type-safe JavaScript interop
- ✅ Access to instance members (services, state)
- ✅ Proper lifecycle management
- ✅ Clean separation of concerns
- ✅ Testable and maintainable

### Why Not Use Static Methods?

Static methods cannot:
- Access injected services (AuthStateProvider, Navigation)
- Access component state (errorMessage, isLoading)
- Call instance methods
- Update UI (StateHasChanged)

### Why JavaScript Function in index.html?

**Why not inline in component?**
- Google's script needs a global function name
- The function must exist when Google's script loads
- Component might not be ready when script loads
- Cleaner separation: setup in HTML, business logic in C#

---

## Security Considerations

### Is Storing Component Reference Safe?

**Yes**, because:
- ✅ Component reference is scoped to the browser session
- ✅ Not accessible from other tabs or users
- ✅ Disposed when component unmounts
- ✅ Only invokable from same-origin JavaScript

### Token Handling

The JWT token from Google:
- Received in browser JavaScript
- Immediately sent to backend for validation
- Not stored in localStorage or sessionStorage
- Backend validates with Google's servers
- Backend creates its own session cookie

---

## Future Improvements

### Potential Enhancements

1. **Better Error Messages**:
   - Differentiate between backend errors and auth errors
   - Show specific Google error messages

2. **Retry Logic**:
   - Automatically retry backend connection
   - Exponential backoff for transient errors

3. **Loading States**:
   - Skeleton loader for button
   - Progress indicator during OAuth flow

4. **Accessibility**:
   - ARIA labels for screen readers
   - Keyboard navigation support
   - Focus management

5. **Analytics**:
   - Track login attempts
   - Monitor error rates
   - Performance metrics

---

## Related Files

- `BlazorApp1/Pages/Login.razor` - Login page component
- `BlazorApp1/Pages/Home.razor` - Home page routing
- `BlazorApp1/wwwroot/index.html` - JavaScript setup
- `BlazorApp1/Auth/CustomAuthStateProvider.cs` - Authentication logic
- `BlazorApp1/Services/ApiService.cs` - Backend API calls

---

## Summary

✅ **Fixed**: Google Sign-In button now appears correctly
✅ **Fixed**: JavaScript interop properly connects to Blazor
✅ **Fixed**: Login flow works end-to-end
✅ **Improved**: Error handling for backend connection issues
✅ **Improved**: User feedback with loading states and error messages

The login button is now visible and functional, even when the backend is not running. Users can authenticate with Google and are properly redirected based on their role.

---

**Last Updated**: After JavaScript interop fix
**Status**: ✅ Working correctly
**Build Status**: ✅ Compiles successfully
