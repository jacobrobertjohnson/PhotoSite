using PhotoSite.Models;

namespace PhotoSite.Library;

public interface ILibraryProvider {
    bool PhotosExist();
    bool VideosExist();
    Task<List<QueryPhoto>> GetPhotos(Family family, string date, string cameraModel);
    Task<List<string>> GetCameraModels(Family family, string date);
    Task<QueryPhoto> GetPhoto(Family family, string photoId);
    Task<List<DateTime>> GetPhotoDates(Family family);
    Task Delete(Family family, string fileId);
}