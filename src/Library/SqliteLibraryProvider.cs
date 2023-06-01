using PhotoSite.Models;
using Microsoft.Data.Sqlite;

namespace PhotoSite.Library;

public class SqliteLibraryProvider : ILibraryProvider {
    ISqliteContext _context;

    public SqliteLibraryProvider(ISqliteContext context)
    {
        _context = context;
    }

    public bool PhotosExist() => true;
    public bool VideosExist() => true;

    public List<QueryPhoto> GetPhotos(Family family, string date) {
        var photos = new List<QueryPhoto>();

        _context.RunQuery(family, $"SELECT FileId, DateTaken, OriginalFilename FROM Photos WHERE DateTaken LIKE '{date}' ORDER BY DateTaken DESC",
            reader => {
                int fileId = reader.GetOrdinal("FileId"),
                    dateTaken = reader.GetOrdinal("DateTaken"),
                    originalFilename = reader.GetOrdinal("OriginalFilename");

                string filename = reader.GetString(originalFilename);

                photos.Add(new QueryPhoto() {
                    Id = reader.GetString(fileId),
                    DateTaken = reader.GetDateTime(dateTaken),
                    OriginalFilename = filename,
                    Extension = Path.GetExtension(filename)
                });
            });

        return photos;
    }

    public QueryPhoto GetPhoto(Family family, string photoId) {
        QueryPhoto photo = new QueryPhoto();

        _context.RunQuery(family, "SELECT FileId, DateTaken, OriginalFilename FROM Photos WHERE FileId = $fileId", command => {
            command.Parameters.AddWithValue("$fileId", photoId).SqliteType = SqliteType.Text;
        }, reader => {
            int fileId = reader.GetOrdinal("FileId"),
                dateTaken = reader.GetOrdinal("DateTaken"),
                originalFilename = reader.GetOrdinal("OriginalFilename");

            string filename = reader.GetString(originalFilename);

            photo.Id = reader.GetString(fileId);
            photo.DateTaken = reader.GetDateTime(dateTaken);
            photo.Extension = Path.GetExtension(filename);
            photo.OriginalFilename = filename;
        });

        return photo;
    }

    public List<DateTime> GetPhotoDates(Family family) {
        var dates = new List<DateTime>();

        _context.RunQuery(family, "SELECT DISTINCT DateTaken FROM Photos ORDER BY DateTaken DESC", reader => {
            int dateTaken = reader.GetOrdinal("DateTaken");

            dates.Add(reader.GetDateTime(dateTaken));
        });

        return dates;
    }
}