using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUglify.Helpers;
using PhotoSite.Authentication;
using PhotoSite.Library;
using PhotoSite.Models;
using PhotoSite.Thumbnails;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.IO;
using System.Text;

namespace PhotoSite.Controllers;

[Authorize]
public class PhotosController : _BaseController
{
    ILibraryProvider _libraryProvider;
    Dictionary<string, Family> _families;

    public PhotosController(IServiceProvider dependencies) : base(dependencies) {
        IAuthenticator authenticator = dependencies.GetService<IAuthenticator>();
        string[] photosFamilies = authenticator.GetClaimValue("photosFamilies").Split(',');

        _families = dependencies.GetService<AppSettings>()
            .Families
            .Where(fam => photosFamilies.Contains(fam.Id))
            .ToDictionary(
                fam => fam.Id,
                fam => fam
            );

        _libraryProvider = dependencies.GetService<ILibraryProvider>();
    }

    [Route("/Photos/{familyId}/{year?}/{month?}")]
    public async Task<IActionResult> Index(
        string familyId,
        int year = -1,
        int month = -1,
        string cameraModel = null
    ) {
        IActionResult response = null;
        string date = "",
            dateLabel = "";
        Family family = _families[familyId];
        List<string> cameraModels = new List<string>();
        List<Photos_Index_SidebarItem> sidebar = await makeSidebar(family.Id);
        string firstMonthUrl = sidebar
            .FirstOrDefault(year => year.Children.Count > 0)?
            .Children?
            .FirstOrDefault()?
            .Url;

        if (year > -1) {
            date = $"%/{year}%";
            dateLabel = $"{year}";

            if (month > -1) {
                date = $"{month}/" + date;
                dateLabel = _monthNames[month] + " " + dateLabel;
            }

            cameraModels = await _libraryProvider.GetCameraModels(family, date);

            if (!cameraModels.Contains(cameraModel))
                cameraModel = null;
        } else if (firstMonthUrl != null){
            response = Redirect(firstMonthUrl);
        }

        response ??= View(new Photos_Index_AspModel(new Photos_Index_VueModel() {
            FamilyId = family.Id
        }) {
            Sidebar = sidebar,
            Thumbnails = await Thumbnails(family.Id, date, cameraModel),
            CameraModels = cameraModels,
            FamilyId = family.Id,
            FamilyName = family.Name,
            DateLabel = dateLabel,
            CameraModel = cameraModel,
            Year = year,
            Month = month
        });

        return response;
    }

    async Task<List<Photos_Index_SidebarItem>> makeSidebar(string familyId) {
        var sidebar = new List<Photos_Index_SidebarItem>();
        bool photosExist = _libraryProvider.PhotosExist(),
            videosExist = _libraryProvider.VideosExist();

        // if (photosExist && videosExist)
        //     sidebar.Add(new Photos_Index_SidebarItem() { Label = "All Photos & Videos" });

        // if (photosExist)
        //     sidebar.Add(new Photos_Index_SidebarItem() { Label = "All Photos" });

        // if (videosExist)
        //     sidebar.Add(new Photos_Index_SidebarItem() { Label = "All Videos" });

        sidebar.AddRange(
            (await _libraryProvider.GetPhotoDates(_families[familyId]))
                .GroupBy(year => year.Year)
                .Select(year => new Photos_Index_SidebarItem() {
                    Label = $"{year.Key}",
                    Url = Url.Action("Index", new {
                        year = year.Key
                    }),
                    Children = year.GroupBy(month => month.Month)
                        .OrderByDescending(month => month.Key)
                        .Select(month => new Photos_Index_SidebarItem() {
                            Label = DateTime.Parse($"{year.Key}-{month.Key}").ToString("MMMM"),
                            Url = Url.Action("Index", new {
                                year = year.Key,
                                month = month.Key
                            })
                        })
                        .ToList()
                })
                .OrderByDescending(year => year.Label)
        );

        return sidebar;
    }

    async Task<List<Photo>> Thumbnails(string familyId, string date, string cameraModel) {
        var family = _families[familyId];
        var dbPhotos = await _libraryProvider.GetPhotos(family, date, cameraModel);

        return Photo.MakeList(familyId, Url, dbPhotos);
    }

    [Route("/Photos/{familyId}/Thumbnails/{size}/{filename}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = ONE_YEAR_IN_SECONDS)]
    public async Task<IActionResult> Thumbnail(string familyId, int size, string filename) {
        Family family = _families[familyId];
        QueryPhoto photo = await getPhotoByFilename(family, filename);

        Thumbnail thumb = new Thumbnail(family, photo, size);

        return File(thumb.Contents, thumb.MimeType);
    }

    [Route("/Photos/{familyId}/FullSize/{filename}")]
    public async Task<IActionResult> FullSize(string familyId, string filename, bool download = false) {
        Family family = _families[familyId];
        string times = "";
        DateTime start = DateTime.Now;
        QueryPhoto photo = await getPhotoByFilename(family, filename);
        times += $"Sqlite: {DateTime.Now - start}. ";
        PhotoReader contents;

        if (download) {
            contents = new PhotoReader(family, photo);
            Response.Headers.ContentDisposition = "attachment; filename=" + photo.OriginalFilename;
        } else {
            contents = new Thumbnail(family, photo, 1080);
        }

        start = DateTime.Now;
        var response = File(contents.Contents, contents.MimeType);
        times += $"Filesystem: {DateTime.Now - start}. ";

        HttpContext.Response.Headers.Add("x-times", times);
        return response;
    }

    [Route("/Photos/{familyId}/Viewer/{filename}")]
    public async Task<IActionResult> Viewer(string familyId, string filename, string cameraModel = null) {
        Family family = _families[familyId];
        string prevPhotoUrl = null,
            nextPhotoUrl = null;
        QueryPhoto photo = await getPhotoByFilename(family, filename);
        var contents = new PhotoReader(family, photo);
        string photoDate = $"{photo.DateTaken.Month}/%/{photo.DateTaken.Year}%";
        List<string> cameraModels = await _libraryProvider.GetCameraModels(family, photoDate);

        if (!cameraModels.Contains(cameraModel))
            cameraModel = null;

        List<QueryPhoto> allPhotos = await _libraryProvider.GetPhotos(family, photoDate, cameraModel);

        for (int i = 0; i < allPhotos.Count; i++) {
            if (allPhotos[i].Id == photo.Id) {
                if (i > 0)
                    prevPhotoUrl = makeViewerUrl(family.Id, allPhotos[i - 1]);
                if (i < allPhotos.Count - 1)
                    nextPhotoUrl = makeViewerUrl(family.Id, allPhotos[i + 1]);
            }
        }

        return View(new Photos_Viewer_AspModel(new object()) {
            PhotoUrl = makePhotoUrl(family.Id, photo),
            PrevPhotoUrl = prevPhotoUrl,
            NextPhotoUrl = nextPhotoUrl,
            Filename = photo.Id + photo.Extension,
            FamilyId = family.Id,
            ExifData = GetExifDataForPhoto(contents),
        });
    }

    List<ExifDatum> GetExifDataForPhoto(PhotoReader contents)
    {
        List<ExifDatum> result = new();

        using (var image = Image.Load(contents.FilePath))
            if (image.Metadata.ExifProfile != null)
                foreach (var exifProp in image.Metadata.ExifProfile.Values)
                    if (ShouldAddExifProp(exifProp))
                        result.Add(new ExifDatum()
                        {
                            Key = $"{exifProp.Tag}",
                            Value = GetExifValue(exifProp.GetValue()),
                        });

        return result
            .OrderBy(GetExifOrder)
            .ToList();
    }

    bool ShouldAddExifProp(IExifValue exifProp)
        => !exifProp.IsArray
            && exifProp.GetValue() != null;

	string GetExifValue(object value)
	{
        if (value == null)
            return "";
       else if (value.GetType() == typeof(string))
        {
            if (DateTime.TryParse((string)value, out DateTime date))
                return date.ToString("dddd, MMMM d, yyyy at hh:mm:ss tt K");
        }

        return $"{value}";
	}

	int GetExifOrder(ExifDatum exifProp)
    {
        Dictionary<string, int> orders = new()
        {
            { "DateTimeOriginal", 0 },
            { "DateTimeDigitized", 10 },
            { "DateTime", 20 },
            { "Make", 30 },
            { "Model", 40 },
            { "ExposureTime", 40 },
            { "FNumber", 50 },
            { "FocalLength", 60 },
        };

        if (orders.ContainsKey(exifProp.Key))
            return orders[exifProp.Key];

        return 99999;
    }

	string makePhotoUrl(string familyId, QueryPhoto photo)
        => $"/Photos/{familyId}/FullSize/{photo.Id}{photo.Extension}";  

    string makeViewerUrl(string familyId, QueryPhoto photo)
        => $"/Photos/{familyId}/Viewer/{photo.Id}{photo.Extension}";  

    [HttpPut]
    [Route("/Photos/{familyId}")]
    public async Task<IActionResult> DeletePhotos(string familyId, [FromBody] DeletePhoto_Request request) {
        Family family = _families[familyId];

        foreach (string filename in request.fileIds) {
            try {
                await deletePhoto(family, filename, request.deletePermanently);
            } catch { }
        }

        return Ok();
    }

    async Task deletePhoto(Family family, string filename, bool deletePermanently) {
        QueryPhoto photo = await getPhotoByFilename(family, filename);

        await _libraryProvider.Delete(family, photo.Id);

        if (deletePermanently) {
            new PhotoReader(family, photo).Delete();
            PhotoSite.Thumbnails.Thumbnail.Delete(family, photo);
        }
    }

    async Task<QueryPhoto> getPhotoByFilename(Family family, string filename) {
        string fileId = Path.GetFileNameWithoutExtension(filename);

        return await _libraryProvider.GetPhoto(family, fileId);
    }
}
