using Microsoft.Data.Sqlite;
using PhotoSite.Crypto;
using PhotoSite.Library;
using PhotoSite.Models;
using Newtonsoft.Json;

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

        Task.WaitAll(_context.RunQuery(_userDbPath, "PRAGMA foreign_keys = ON"));

        Task.WaitAll(_context.RunQuery(_userDbPath,
            "CREATE TABLE IF NOT EXISTS User(" +
                "UserId INTEGER PRIMARY KEY," +
                "Username TEXT," +
                "Password TEXT," +
                "Enabled INTEGER, " +
                "UserAdmin INTEGER " +
            ")"
        ));

        Task.WaitAll(_context.RunQuery(_userDbPath,
            "CREATE TABLE IF NOT EXISTS User_Family(" +
                "UserId INTEGER," +
                "FamilyName TEXT," +
                "Permissions TEXT," +

                "FOREIGN KEY(UserId) REFERENCES User(UserId) ON DELETE CASCADE" +
            ")"
        ));
    }

    public async Task<AuthenticatedUser> Authenticate(string username, string plainPassword) {
        AuthenticatedUser user = null;
        string hashedPassword =  _cryptoProvider.HashValue(plainPassword, username, _machineKey);

        await _context.RunQuery(_userDbPath,
            "SELECT U.UserId, U.Password, U.Username, U.UserAdmin, UF.FamilyName, UF.Permissions " +
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
                    userAdmin = reader.GetOrdinal("UserAdmin"),
                    permissions = reader.GetOrdinal("Permissions");

                string family = reader.GetString(familyName),
                    storedHashedPassword = reader.GetString(password);

                if (_cryptoProvider.CompareHashes(hashedPassword, storedHashedPassword)) {
                    user = user ?? new AuthenticatedUser();
                    user.Username = reader.GetString(username);
                    user.UserId = reader.GetInt32(userId);
                    user.UserAdmin = reader.GetBoolean(userAdmin);
                    user.Families[family] = JsonConvert.DeserializeObject<UserPermissions>(reader.GetString(permissions));
                }
            }
        );

        return user;
    }

    public async Task SetPassword(int userId, string hashedNewPassword) {
        await _context.RunQuery(_userDbPath,
            "UPDATE User " +
                "SET Password = $password " +
            "WHERE UserId = $userId",

            command => {
                command.Parameters.AddWithValue("$userId", userId).SqliteType = SqliteType.Integer;
                command.Parameters.AddWithValue("$password", hashedNewPassword).SqliteType = SqliteType.Text;
            },

            reader => { }
        );
    }

    public async Task<Users_Index_User[]> GetAllUsers() {
        List<Users_Index_User> users = new();

        await _context.RunQuery(_userDbPath,
            "SELECT UserId, Username FROM User",

            command => {},

            reader => {
                int userId = reader.GetOrdinal("UserId"),
                    username = reader.GetOrdinal("Username");

                users.Add(new() {
                    UserId = reader.GetInt32(userId),
                    Username = reader.GetString(username),
                });
            }
        );

        return users.ToArray();
    }

    public async Task<Users_User_Model> GetUser(int userId) {
        Users_User_Model user = new();

        await _context.RunQuery(_userDbPath,
            "SELECT U.UserId, U.Username, U.Enabled, U.UserAdmin, UF.FamilyName, UF.Permissions " +
            "FROM   User U " +
                "LEFT JOIN   User_Family UF " +
                    "ON U.UserId = UF.UserId " +
            "WHERE  U.UserId = $userId ",

            command => {
                command.Parameters.AddWithValue("$userId", userId).SqliteType = SqliteType.Integer;
            },

            reader => {
                int userId = reader.GetOrdinal("UserId"),
                    username = reader.GetOrdinal("Username"),
                    enabled = reader.GetOrdinal("Enabled"),
                    userAdmin = reader.GetOrdinal("UserAdmin"),
                    familyName = reader.GetOrdinal("FamilyName"),
                    permissions = reader.GetOrdinal("Permissions");

                user.UserId = reader.GetInt32(userId);
                user.Username = reader.GetString(username);
                user.Enabled = reader.GetBoolean(enabled);
                user.UserAdmin = reader.GetBoolean(userAdmin);

                if (!reader.IsDBNull(familyName))
                    user.Permissions[reader.GetString(familyName)] = JsonConvert.DeserializeObject<UserPermissions>(reader.GetString(permissions));
            }
        );

        return user;
    }

    public async Task<int> AddUser(Users_User_Model body) {
        string hashedPassword = _cryptoProvider.HashValue(body.NewPassword, body.Username, _machineKey);
        int userId = 0;

        await _context.RunQuery(_userDbPath,
            "INSERT INTO User (Username, Password, Enabled, UserAdmin) " +
            "VALUES ($username, $password, $enabled, $userAdmin) ",

            command => {
                command.Parameters.AddWithValue("$username", body.Username).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$password", hashedPassword).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$enabled", body.Enabled ? 1 : 0).SqliteType = SqliteType.Integer;
                command.Parameters.AddWithValue("$userAdmin", body.UserAdmin ? 1 : 0).SqliteType = SqliteType.Integer;
            },

            reader => {}
        );

        await _context.RunQuery(_userDbPath,
            "SELECT last_insert_rowid() AS UserId",
            command => {},
            reader => {
                userId = reader.GetInt32(reader.GetOrdinal("UserId"));
            });

        return userId;
    }

    public async Task UpdateUser(int userId, Users_User_Model body) {
        await _context.RunQuery(_userDbPath,
            "UPDATE User " +
                "SET Enabled = $enabled " +
            "WHERE  UserId = $userId ",

            command => {
                command.Parameters.AddWithValue("$userId", userId).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$enabled", body.Enabled ? 1 : 0).SqliteType = SqliteType.Integer;
            },

            reader => {}
        );
    }

    public async Task SetUserFamilyPermissions(int userId, string familyName, UserPermissions permissions) {
        string strPerms = JsonConvert.SerializeObject(permissions);

        if (await userFamilyPermissionsExist(userId, familyName))
            await updateUserFamilyPermissions(userId, familyName, strPerms);
        else
            await addUserFamilyPermissions(userId, familyName, strPerms);
    }

    private async Task<bool> userFamilyPermissionsExist(int userId, string familyName) {
        bool permsExist = false;

        await _context.RunQuery(_userDbPath,
            "SELECT 1 " +
            "FROM   User_Family " +
            "WHERE  UserId = $userId " +
                "AND    FamilyName = $familyName",

            command => {
                command.Parameters.AddWithValue("$userId", userId).SqliteType = SqliteType.Integer;
                command.Parameters.AddWithValue("$familyName", familyName).SqliteType = SqliteType.Text;
            },

            reader => {
                permsExist = true;
            }
        );

        return permsExist;
    }

    private async Task updateUserFamilyPermissions(int userId, string familyName, string permissions)
        => await _context.RunQuery(_userDbPath,
            "UPDATE User_Family " +
                "SET    Permissions = $permissions " +
            "WHERE  UserId = $userId " +
                "AND    FamilyName = $familyName",

            command => {
                command.Parameters.AddWithValue("$userId", userId).SqliteType = SqliteType.Integer;
                command.Parameters.AddWithValue("$familyName", familyName).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$permissions", permissions).SqliteType = SqliteType.Text;
            },

            reader => { }
        );

    private async Task addUserFamilyPermissions(int userId, string familyName, string permissions)
        => await _context.RunQuery(_userDbPath,
            "INSERT INTO User_Family (UserId, FamilyName, Permissions) " +
            "VALUES ($userId, $familyName, $permissions) ",

            command => {
                command.Parameters.AddWithValue("$userId", userId).SqliteType = SqliteType.Integer;
                command.Parameters.AddWithValue("$familyName", familyName).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$permissions", permissions).SqliteType = SqliteType.Text;
            },

            reader => { }
        );
}