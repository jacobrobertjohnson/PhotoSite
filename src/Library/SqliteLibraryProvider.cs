namespace PhotoSite.Library;

public class SqliteLibraryProvider : ILibraryProvider {
    public bool PhotosExist() => true;
    public bool VideosExist() => true;
}