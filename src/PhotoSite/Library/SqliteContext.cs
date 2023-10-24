using Microsoft.Data.Sqlite;
using PhotoSite.Models;

namespace PhotoSite.Library;

public class SqliteContext : ISqliteContext {
    public SqliteContext() { }

    public void RunQuery(Family family, string query) => RunQuery(family, query, (reader) => { });

    public void RunQuery(string dbPath, string query) => RunQuery(dbPath, query, (reader) => { });

    public void RunQuery(Family family, string query, Action<SqliteDataReader> onRun) 
        => RunQuery(family, query, command => {}, onRun);
    
    public void RunQuery(string dbPath, string query, Action<SqliteDataReader> onRun) 
        => RunQuery(dbPath, query, command => {}, onRun);

    public void RunQuery(Family family, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun)
        => RunQuery(family.PhotoDbPath, query, commandFunc, onRun);

    public void RunQuery(string dbPath, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun) {
        using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}")) {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = query;
                commandFunc(command);

                using (SqliteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read())
                        onRun(reader);
                }
            }
        }
    }
}