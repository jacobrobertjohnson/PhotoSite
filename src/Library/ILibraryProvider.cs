using PhotoSite.Models;

namespace PhotoSite.Library;

public interface ILibraryProvider {
    bool PhotosExist();
    bool VideosExist();
    List<QueryPhoto> GetPhotos(Family family, string date);
    QueryPhoto GetPhoto(Family family, string photoId);
    List<DateTime> GetPhotoDates(Family family);
}