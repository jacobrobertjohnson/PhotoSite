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

    [Route("/Photos/{familyId}")]
    public IActionResult Index(string familyId) {
        IActionResult response = RedirectToAction("Index", "Home");

        if (_families.ContainsKey(familyId))
            response = View(new ModelBase(new Photos_Index_VueModel() {
                FamilyId = familyId,
                Sidebar = new List<Photos_Index_SidebarItem>() {
                    new Photos_Index_SidebarItem() { Label = "All Photos & Videos" },
                    new Photos_Index_SidebarItem() { Label = "All Photos" },
                    new Photos_Index_SidebarItem() { Label = "All Videos" },
                }
            }));


        return response;
    }

    List<Photos_Index_SidebarItem> makeSidebar() {
        var sidebar = new List<Photos_Index_SidebarItem>();
        bool photosExist = _libraryProvider.PhotosExist(),
            videosExist = _libraryProvider.VideosExist();

        if (photosExist && videosExist)
            sidebar.Add(new Photos_Index_SidebarItem() { Label = "All Photos & Videos" });

        if (photosExist)
            sidebar.Add(new Photos_Index_SidebarItem() { Label = "All Photos" });

        if (videosExist)
            sidebar.Add(new Photos_Index_SidebarItem() { Label = "All Videos" });

        return sidebar;
    }

    [Route("/Photos/{familyId}/Thumbnails")]
    public IActionResult Thumbnails(string familyId) {
        var family = _families[familyId];
        var dbPhotos = _libraryProvider.GetPhotos(family);

        return Json(Photo.MakeList(familyId, Url, dbPhotos));
    }

    [Route("/Photos/{familyId}/Thumbnails/{size}/{fileId}")]
    public IActionResult Thumbnail(string familyId, int size, string fileId) {
        Family family = _families[familyId];
        QueryPhoto photo = _libraryProvider.GetPhoto(family, fileId);

        Thumbnail thumb = new Thumbnail(family, photo, size);

        return File(thumb.ThumbnailContents, thumb.ThumbnailMimeType);
    }
}
