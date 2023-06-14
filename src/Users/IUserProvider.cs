using PhotoSite.Models;

namespace PhotoSite.Users;

public interface IUserProvider {
    AuthenticatedUser Authenticate(string username, string plainPassword);
    void SetPassword(int userId, string hashedNewPassword);
}