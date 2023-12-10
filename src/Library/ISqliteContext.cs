using Microsoft.Data.Sqlite;
using PhotoSite.Models;

namespace PhotoSite.Library;

public interface ISqliteContext {
    Task RunQuery(Family family, string query);
    Task RunQuery(string dbPath, string query);
    Task RunQuery(Family family, string query, Action<SqliteDataReader> onRun);
    Task RunQuery(string dbPath, string query, Action<SqliteDataReader> onRun);
    Task RunQuery(Family family, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun);
    Task RunQuery(string dbPath, string query, Action<SqliteCommand> commandFunc, Action<SqliteDataReader> onRun);
}