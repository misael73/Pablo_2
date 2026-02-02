using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Sirefi.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID claim not found. Authentication required.");
        }
        
        if (!int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format in claims.");
        }
        
        return userId;
    }
    
    protected string GetCurrentUserRole()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(role))
        {
            throw new UnauthorizedAccessException("User role claim not found. Authentication required.");
        }
        
        return role;
    }
}
