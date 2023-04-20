namespace PhotoSite.Library;

public interface ILibraryProvider {
    bool PhotosExist();
    bool VideosExist();
}