namespace PhotoSite.Models;

public class AppSettings {
    public List<Family> Families { get; set; } = new List<Family>();
}

public class Family {
    public string Id { get; set; }
    public string Name { get; set; }
    public string PhotoDbPath { get; set; }
    public string PhotoFilePath { get; set; }
    public string PhotoThumbnailPath { get; set; }
}