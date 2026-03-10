using System.Security.Claims;

namespace BBMB.BudgetService.Shared.Utilities
{
    public static class JwtClaimHelper
    {
        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
        }

        public static string? GetUserName(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value
                   ?? user.FindFirst("unique_name")?.Value;
        }

        public static string? GetJti(ClaimsPrincipal user)
        {
            return user.FindFirst("jti")?.Value;
        }

        public static IEnumerable<Claim> GetAllClaims(ClaimsPrincipal user)
        {
            return user.Claims;
        }

        public static Dictionary<string, object> GetAllClaimsAsDictionary(ClaimsPrincipal user)
        {
            return user.Claims
                       .GroupBy(c => c.Type)
                       .ToDictionary(
                           g => g.Key,
                           g => g.Count() > 1
                               ? g.Select(x => x.Value).ToList<object>()
                               : g.First().Value as object
                       );
        }
    }
}
