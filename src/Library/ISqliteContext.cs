using Microsoft.Data.Sqlite;
using PhotoSite.Models;

namespace PhotoSite.Library;

public interface ISqliteContext {
    void RunQuery(Family family, string query);
    void RunQuery(Family family, string query, Action<SqliteDataReader> onRun);
}