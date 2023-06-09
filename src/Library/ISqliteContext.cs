using Microsoft.Data.Sqlite;
using PhotoSite.Models;

namespace PhotoSite.Library;

public interface ISqliteContext {
    void RunQuery(Family family, string query);
    void RunQuery(string dbPath, string query);
    void RunQuery(Family family, string query, Action<SqliteDataReader> onRun);
    void RunQuery(string dbPath, string query, Action<SqliteDataReader> onRun);
    void RunQuery(Family family, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun);
    void RunQuery(string dbPath, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun);
}