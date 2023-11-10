using System.Security.Claims;

namespace Emergency.Extensions
{
    public static class ClaimsPrincipalExt
    {
        public static int GetId(this ClaimsPrincipal user)
        {
            var idStr = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out int id))
            {
                return 0;
            }
            return id;
        }

        public static string GetName(this ClaimsPrincipal user)
        {
            return user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
