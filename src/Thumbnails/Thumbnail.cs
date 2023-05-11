using PhotoSite.Models;

namespace PhotoSite.Thumbnails;

public class Thumbnail {
    readonly int[] _heightBreakpoints = new[] {
        10,
        60,
        120,
        150,
        300,
        500
    };

    public Thumbnail(Family family, DateTime date, int size, string filename) {
        int targetSize = getNearestHeightBreakpoint(size);
        string thumbnailPath = makeThumbnailPath(family, date, targetSize, filename);

        if (!File.Exists(thumbnailPath))
            resizeAndCacheThumbnail(targetSize, makeFullSizePath(family, date, filename), thumbnailPath);

        ThumbnailContents = File.ReadAllBytes(thumbnailPath);
        ThumbnailMimeType = "image/jpeg";
    }

    public byte[] ThumbnailContents { get; private set; }
    public string ThumbnailMimeType { get; private set; } 

    int getNearestHeightBreakpoint(int size) {
        int? nearestBreakpoint = null;

        foreach (int breakpoint in _heightBreakpoints) {
            if (size <= breakpoint)
                nearestBreakpoint = breakpoint;
        }

        return nearestBreakpoint ?? _heightBreakpoints.Last();
    }

    string makeFullSizePath(Family family, DateTime date, string filename)
        => makeImagePath(family.PhotoFilePath, date, filename);

    string makeThumbnailPath(Family family, DateTime date, int targetSize, string filename)
        => makeImagePath(Path.Combine(family.PhotoThumbnailPath, $"{targetSize}"), date, filename);

    string makeImagePath(string baseDir, DateTime date, string filename) {
        string dirDate = date.ToString("yyyy-MM"),
            fullFilename = $"{date:yyyy-MM-dd}_{filename}";

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