using PhotoSite.Models;

namespace PhotoSite.Thumbnails;

public class Thumbnail : PhotoReader {
    readonly int[] _heightBreakpoints = new[] {
        100,
        200,
        300,
    };

    public Thumbnail(Family family, QueryPhoto photo, int size) : base(family, photo) {
        int targetSize = getNearestHeightBreakpoint(size);

        _photoPath = makeThumbnailPath(family, photo, targetSize);

        if (!File.Exists(_photoPath))
            resizeAndCacheThumbnail(targetSize);
    }

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

    string makeThumbnailPath(Family family, QueryPhoto photo, int targetSize)
        => makeImagePath(Path.Combine(family.PhotoThumbnailPath, $"{targetSize}"), photo);

    void resizeAndCacheThumbnail(int newHeight) {
        string targetDir = Path.GetDirectoryName(_photoPath);

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        using (var stream = new FileStream(_fullsizePath, FileMode.Open))
        using (var image = Image.Load(stream))
        {
            int width = (image.Width * newHeight) / image.Height;
            
            image.Mutate(x => x.Resize(width, newHeight, KnownResamplers.Lanczos3));
            image.Save(_photoPath);
        }
    }
}