using System.Security.Claims;

namespace PhotoSite.Authentication;

public class Authenticator : IAuthenticator {
    IHttpContextAccessor _contextAccessor;

    public Authenticator(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public List<Claim> GetClaims() => _contextAccessor.HttpContext.User.Claims.ToList();
    public Claim GetClaim(string type) => GetClaims().FirstOrDefault(claim => claim.Type == type);
    public string GetClaimValue(string type) => GetClaim(type)?.Value;
}