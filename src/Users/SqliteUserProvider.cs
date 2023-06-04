using Microsoft.Data.Sqlite;
using PhotoSite.Library;
using PhotoSite.Models;

namespace PhotoSite.Users;

public class SqliteUserProvider : IUserProvider {
    string _userDbPath;
    ISqliteContext _context;

    public SqliteUserProvider(IServiceProvider dependencies) {
        _userDbPath = dependencies.GetService<AppSettings>().UserDbPath;
        _context = dependencies.GetService<ISqliteContext>();

        _context.RunQuery(_userDbPath, "PRAGMA foreign_keys = ON");

        _context.RunQuery(_userDbPath,
            "CREATE TABLE IF NOT EXISTS User(" +
                "UserId INTEGER PRIMARY KEY," +
                "Username TEXT," +
                "Password TEXT," +
                "Enabled INTEGER" +
            ")"
        );

        _context.RunQuery(_userDbPath,
            "CREATE TABLE IF NOT EXISTS User_Family(" +
                "UserId INTEGER," +
                "FamilyName TEXT," +
                "Photos INTEGER," +
                "DeletePhotos INTEGER," +
                "DeletePhotosPermanently INTEGER," +

                "FOREIGN KEY(UserId) REFERENCES User(UserId) ON DELETE CASCADE" +
            ")"
        );
    }

    public AuthenticatedUser Authenticate(string username, string plainPassword) {
        AuthenticatedUser user = null;

        _context.RunQuery(_userDbPath,
            "SELECT U.UserId, Username, FamilyName, Photos, DeletePhotos, DeletePhotosPermanently " +
            "FROM User U " +
                "JOIN User_Family UF " +
                    "ON U.UserId = UF.UserId " +
            "WHERE Username = $username " +
                "AND Enabled = 1",

            command => command.Parameters.AddWithValue("$username", username).SqliteType = SqliteType.Text,

            reader => {
                int userId = reader.GetOrdinal("UserId"),
                    username = reader.GetOrdinal("Username"),
                    familyName = reader.GetOrdinal("FamilyName"),
                    photos = reader.GetOrdinal("Photos"),
                    deletePhotos = reader.GetOrdinal("DeletePhotos"),
                    deletePhotosPermanently = reader.GetOrdinal("DeletePhotosPermanently");

                string family = reader.GetString(familyName);

                user = user ?? new AuthenticatedUser();
                user.Username = reader.GetString(username);
                user.Families.Add(family);

                if (reader.GetBoolean(photos)) {
                    user.PhotoFamilies.Add(family);

                    if (reader.GetBoolean(deletePhotos))
                        user.PhotoDeleteFamilies.Add(family);
                    
                    if (reader.GetBoolean(deletePhotosPermanently))
                        user.PhotoPermanentDeleteFamilies.Add(family);
                }
            }
        );

        return user;
    }
}