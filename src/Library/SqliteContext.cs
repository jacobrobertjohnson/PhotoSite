using Microsoft.Data.Sqlite;
using PhotoSite.Models;

namespace PhotoSite.Library;

public class SqliteContext : ISqliteContext {
    public SqliteContext() { }

    public async Task RunQuery(Family family, string query) => await RunQuery(family, query, (reader) => { });

    public async Task RunQuery(string dbPath, string query) => await RunQuery(dbPath, query, (reader) => { });

    public async Task RunQuery(Family family, string query, Action<SqliteDataReader> onRun) 
        => await RunQuery(family, query, command => {}, onRun);
    
    public async Task RunQuery(string dbPath, string query, Action<SqliteDataReader> onRun) 
        => await RunQuery(dbPath, query, command => {}, onRun);

    public async Task RunQuery(Family family, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun)
        => await RunQuery(family.PhotoDbPath, query, commandFunc, onRun);

    public async Task RunQuery(string dbPath, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun) {
        using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}")) {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = query;
                commandFunc(command);

                using (SqliteDataReader reader = await command.ExecuteReaderAsync()) {
                    while (reader.Read())
                        onRun(reader);
                }
            }
        }
    }
}