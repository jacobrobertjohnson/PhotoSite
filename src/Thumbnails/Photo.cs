using PhotoSite.Models;

namespace PhotoSite.Thumbnails;

public class PhotoReader {
    protected string _fullsizePath,
        _photoPath;

    public PhotoReader(Family family, QueryPhoto photo) {
        _fullsizePath = makeFullSizePath(family, photo);
        _photoPath = _fullsizePath;
    }

    public byte[] Contents { get => File.ReadAllBytes(_photoPath); }
    public string MimeType { get; private set; } = "image/jpeg";

    string makeFullSizePath(Family family, QueryPhoto photo)
        => makeImagePath(family.PhotoFilePath, photo);

    protected string makeImagePath(string baseDir, QueryPhoto photo) {
        string dirDate = photo.DateTaken.ToString("yyyy-MM"),
            fullFilename = $"{photo.DateTaken:yyyy-MM-dd}_{photo.Id}{photo.Extension}";

        return Path.Combine(baseDir, dirDate, fullFilename);
    }

    public void Delete() => delete(_photoPath);

    protected void delete(string photoPath) {
        try {
            File.Delete(photoPath);
        } catch { }

        deleteFolderIfEmpty(_photoPath);
    }

    private void deleteFolderIfEmpty(string photoPath) {
        string dirName = Path.GetDirectoryName(photoPath);
        
        try {
            if (Directory.GetFiles(dirName).Count() == 0)
                Directory.Delete(dirName);
        } catch { }
    }
}