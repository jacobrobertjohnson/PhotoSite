using PhotoSite.Models;

namespace PhotoSite.Library;

public class SqliteLibraryProvider : ILibraryProvider {
    ISqliteContext _context;

    public SqliteLibraryProvider(ISqliteContext context)
    {
        _context = context;
    }

    public bool PhotosExist() => true;
    public bool VideosExist() => true;

    public List<QueryPhoto> GetPhotos(Family family) {
        var photos = new List<QueryPhoto>();

        _context.RunQuery(family, "SELECT FileId, DateTaken, OriginalFilename FROM Photos", reader => {
            int fileId = reader.GetOrdinal("FileId"),
                dateTaken = reader.GetOrdinal("DateTaken"),
                originalFilename = reader.GetOrdinal("OriginalFilename");

            photos.Add(new QueryPhoto() {
                Id = reader.GetString(fileId),
                DateTaken = reader.GetDateTime(dateTaken),
                Extension = Path.GetExtension(reader.GetString(originalFilename))
            });
        });

        return photos;
    }
}