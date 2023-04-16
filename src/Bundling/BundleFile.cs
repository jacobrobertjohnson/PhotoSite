using Newtonsoft.Json;

namespace PhotoSite.Bundling;

public class BundleFile {
    string _webRootPath,
        _bundlePath;

    public BundleFile(IWebHostEnvironment environment)
    {
        _webRootPath = environment.WebRootPath;
        _bundlePath = Path.Combine(_webRootPath, "..", "bundleconfig.json");
    }

    public string[] ExpandBundle(string path) {
        string bundleConfig = File.ReadAllText(_bundlePath);    
        Bundle[] bundles = JsonConvert.DeserializeObject<Bundle[]>(bundleConfig);
        string[] bundleContents = new string[0];

        foreach (dynamic bundle in bundles) {
            if (bundle.outputFileName == path) {
                bundleContents = bundle.inputFiles as string[];
                break;
            }
        }

        return bundleContents
            .Select(f => f.Replace("wwwroot", "~"))
            .ToArray();
    }
}