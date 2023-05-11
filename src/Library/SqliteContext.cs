using Microsoft.Data.Sqlite;
using PhotoSite.Models;

namespace PhotoSite.Library;

public class SqliteContext : ISqliteContext {
    public SqliteContext() { }

    public void RunQuery(Family family, string query) => RunQuery(family, query, (reader) => { });

    public void RunQuery(Family family, string query, Action<SqliteDataReader> onRun) {
        using (SqliteConnection connection = new SqliteConnection($"Data Source={family.PhotoDbPath}")) {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = query;

                using (SqliteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read())
                        onRun(reader);
                }
            }
        }
    }
}