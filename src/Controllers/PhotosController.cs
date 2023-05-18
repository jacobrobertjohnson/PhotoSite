using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Authentication;
using PhotoSite.Library;
using PhotoSite.Models;
using PhotoSite.Thumbnails;
using System.IO;

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
    public IActionResult Index(string familyId, int year = -1, int month = -1) {
        IActionResult response = RedirectToAction("Index", "Home");

        if (_families.ContainsKey(familyId))
            response = View(new Photos_Index_AspModel(makeSidebar(familyId), new Photos_Index_VueModel() {
                FamilyId = familyId
            }));

        return response;
    }

    List<Photos_Index_SidebarItem> makeSidebar(string familyId) {
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
            _libraryProvider.GetPhotoDates(_families[familyId])
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

    [Route("/Photos/{familyId}/Thumbnails")]
    public IActionResult Thumbnails(string familyId) {
        var family = _families[familyId];
        var dbPhotos = _libraryProvider.GetPhotos(family);

        return Json(Photo.MakeList(familyId, Url, dbPhotos));
    }

    [Route("/Photos/{familyId}/Thumbnails/{size}/{filename}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = ONE_YEAR_IN_SECONDS)]
    public IActionResult Thumbnail(string familyId, int size, string filename) {
        Family family = _families[familyId];
        string fileId = Path.GetFileNameWithoutExtension(filename);
        QueryPhoto photo = _libraryProvider.GetPhoto(family, fileId);

        Thumbnail thumb = new Thumbnail(family, photo, size);

        return File(thumb.ThumbnailContents, thumb.ThumbnailMimeType);
    }
}
