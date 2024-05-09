using PhotoSite.Models;

namespace PhotoSite.Users;

public interface IUserProvider {
    Task<AuthenticatedUser> Authenticate(string username, string plainPassword);
    Task SetPassword(int userId, string hashedNewPassword);
    Task<Users_Index_User[]> GetAllUsers();
    Task<Users_User_Model> GetUser(int userId);
    Task<int> AddUser(Users_User_Model body);
    Task UpdateUser(int userId, Users_User_Model body);
    Task SetUserFamilyPermissions(int userId, string familyName, UserPermissions permissions);
}