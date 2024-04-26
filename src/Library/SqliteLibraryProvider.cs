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

    public async Task<List<QueryPhoto>> GetPhotos(Family family, string date, string cameraModel) {
        var photos = new List<QueryPhoto>();
        string query = $"SELECT FileId, DateTaken, OriginalFilename FROM Photos WHERE Deleted = 0 AND DateTaken LIKE '{date}'";

        if (!string.IsNullOrWhiteSpace(cameraModel))
            query += " AND ExifModel = $cameraModel";

        await _context.RunQuery(family,  query,
            command => command.Parameters.AddWithValue("$cameraModel", cameraModel).SqliteType = SqliteType.Text,
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

    public async Task<List<string>> GetCameraModels(Family family, string date) {
        var photos = new List<string>();

        await _context.RunQuery(family, $"SELECT DISTINCT ExifModel FROM Photos WHERE Deleted = 0 AND DateTaken LIKE '{date}' ORDER BY ExifModel ASC",
            reader => {
                int exifModel = reader.GetOrdinal("ExifModel");

                if (!reader.IsDBNull(exifModel))
                    photos.Add(reader.GetString(exifModel));
            });

        return photos;
    }

    public async Task<QueryPhoto> GetPhoto(Family family, string photoId) {
        QueryPhoto photo = new QueryPhoto();

        await _context.RunQuery(family, "SELECT FileId, DateTaken, OriginalFilename FROM Photos WHERE Deleted = 0 AND FileId = $fileId", command => {
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
        }).ConfigureAwait(false);

        return photo;
    }

    public async Task<List<DateTime>> GetPhotoDates(Family family) {
        var dates = new List<DateTime>();

        await _context.RunQuery(family, "SELECT DISTINCT DateTaken FROM Photos WHERE Deleted = 0 ORDER BY DateTaken DESC", reader => {
            int dateTaken = reader.GetOrdinal("DateTaken");

            dates.Add(reader.GetDateTime(dateTaken));
        });

        return dates;
    }

    public async Task Delete(Family family, string fileId) {
        await _context.RunQuery(family, "UPDATE Photos SET Deleted = 1 WHERE FileId = $fileId",
            command => command.Parameters.AddWithValue("$fileId", fileId).SqliteType = SqliteType.Text,
            reader => { }
        );
    }
}