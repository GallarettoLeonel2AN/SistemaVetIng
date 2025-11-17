using System.Security.Claims;

public static class PermissionChecker
{
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        
        if (user.HasClaim("DenyPermission", permission))
            return false;

        return user.HasClaim("Permission", permission);
    }
}
