using PhotoSite.Models;

namespace PhotoSite.Thumbnails;

public class Thumbnail {
    readonly int[] _heightBreakpoints = new[] {
        60,
        100,
        150,
        300,
        500
    };

    public Thumbnail(Family family, QueryPhoto photo, int size) {
        int targetSize = getNearestHeightBreakpoint(size);
        string thumbnailPath = makeThumbnailPath(family, photo, targetSize);

        if (!File.Exists(thumbnailPath))
            resizeAndCacheThumbnail(targetSize, makeFullSizePath(family, photo), thumbnailPath);

        ThumbnailContents = File.ReadAllBytes(thumbnailPath);
        ThumbnailMimeType = "image/jpeg";
    }

    public byte[] ThumbnailContents { get; private set; }
    public string ThumbnailMimeType { get; private set; } 

    int getNearestHeightBreakpoint(int size) {
        int? nearestBreakpoint = null;

        foreach (int breakpoint in _heightBreakpoints) {
            if (size >= breakpoint)
                nearestBreakpoint = breakpoint;
        }

        if (nearestBreakpoint == null)
            if (size < _heightBreakpoints.First())
                nearestBreakpoint = _heightBreakpoints.First();
            else
                nearestBreakpoint = _heightBreakpoints.Last();

        return nearestBreakpoint.Value;
    }

    string makeFullSizePath(Family family, QueryPhoto photo)
        => makeImagePath(family.PhotoFilePath, photo);

    string makeThumbnailPath(Family family, QueryPhoto photo, int targetSize)
        => makeImagePath(Path.Combine(family.PhotoThumbnailPath, $"{targetSize}"), photo);

    string makeImagePath(string baseDir, QueryPhoto photo) {
        string dirDate = photo.DateTaken.ToString("yyyy-MM"),
            fullFilename = $"{photo.DateTaken:yyyy-MM-dd}_{photo.Id}{photo.Extension}";

        return Path.Combine(baseDir, dirDate, fullFilename);
    }

    void resizeAndCacheThumbnail(int newHeight, string sourcePath, string targetPath) {
        string targetDir = Path.GetDirectoryName(targetPath);

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        using (var stream = new FileStream(sourcePath, FileMode.Open))
        using (var image = Image.Load(stream))
        {
            int width = (image.Width * newHeight) / image.Height;
            
            image.Mutate(x => x.Resize(width, newHeight, KnownResamplers.Lanczos3));
            image.Save(targetPath);
        }
    }
}