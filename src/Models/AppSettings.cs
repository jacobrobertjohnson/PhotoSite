namespace PhotoSite.Models;

public class AppSettings {
    public string UserDbPath { get; set; }
    public List<Family> Families { get; set; } = new List<Family>();
    public string MachineKey { get; set; }
    public string DataProtectionKeyPath { get; set; }
    public string ErrorLogPath { get; set; }
}

public class Family {
    public string Id { get; set; }
    public string Name { get; set; }
    public string PhotoDbPath { get; set; }
    public string PhotoFilePath { get; set; }
    public string PhotoThumbnailPath { get; set; }
}