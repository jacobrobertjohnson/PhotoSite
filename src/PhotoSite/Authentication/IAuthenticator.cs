using System.Security.Claims;

namespace PhotoSite.Authentication;

public interface IAuthenticator {
    List<Claim> GetClaims();
    Claim GetClaim(string type);
    string GetClaimValue(string type);
}