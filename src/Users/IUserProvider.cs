using PhotoSite.Models;

namespace PhotoSite.Users;

public interface IUserProvider {
    Task<AuthenticatedUser> Authenticate(string username, string plainPassword);
    Task SetPassword(int userId, string hashedNewPassword);
}