using Microsoft.Data.Sqlite;
using PhotoSite.Crypto;
using PhotoSite.Library;
using PhotoSite.Models;

namespace PhotoSite.Users;

public class SqliteUserProvider : IUserProvider {
    string _userDbPath,
        _machineKey;
    ISqliteContext _context;
    ICryptoProvider _cryptoProvider;

    public SqliteUserProvider(IServiceProvider dependencies) {
        AppSettings config = dependencies.GetService<AppSettings>();
        
        _machineKey = config.MachineKey;
        _userDbPath = config.UserDbPath;
        _context = dependencies.GetService<ISqliteContext>();
        _cryptoProvider = dependencies.GetService<ICryptoProvider>();

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
        string hashedPassword =  _cryptoProvider.HashValue(plainPassword, username, _machineKey);

        _context.RunQuery(_userDbPath,
            "SELECT U.UserId, Password, Username, FamilyName, Photos, DeletePhotos, DeletePhotosPermanently " +
            "FROM User U " +
                "JOIN User_Family UF " +
                    "ON U.UserId = UF.UserId " +
            "WHERE Username = $username " +
                "AND Enabled = 1",

            command => command.Parameters.AddWithValue("$username", username).SqliteType = SqliteType.Text,

            reader => {
                int userId = reader.GetOrdinal("UserId"),
                    password = reader.GetOrdinal("Password"),
                    username = reader.GetOrdinal("Username"),
                    familyName = reader.GetOrdinal("FamilyName"),
                    photos = reader.GetOrdinal("Photos"),
                    deletePhotos = reader.GetOrdinal("DeletePhotos"),
                    deletePhotosPermanently = reader.GetOrdinal("DeletePhotosPermanently");

                string family = reader.GetString(familyName),
                    storedHashedPassword = reader.GetString(password);

                if (_cryptoProvider.CompareHashes(hashedPassword, storedHashedPassword)) {
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
            }
        );

        return user;
    }
}