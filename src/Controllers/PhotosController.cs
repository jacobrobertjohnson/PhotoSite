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
        string date = "";

        if (_families.ContainsKey(familyId)) {
            if (year > -1) {
                date = $"%/{year}%";

                if (month > -1)
                    date = $"{month}/" + date;
            }

            response = View(new Photos_Index_AspModel(new Photos_Index_VueModel() {
                FamilyId = familyId
            }) {
                Sidebar = makeSidebar(familyId),
                Thumbnails = Thumbnails(familyId, date),
                FamilyId = familyId
            });
        }

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

    List<Photo> Thumbnails(string familyId, string date) {
        var family = _families[familyId];
        var dbPhotos = _libraryProvider.GetPhotos(family, date);

        return Photo.MakeList(familyId, Url, dbPhotos);
    }

    [Route("/Photos/{familyId}/Thumbnails/{size}/{filename}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = ONE_YEAR_IN_SECONDS)]
    public IActionResult Thumbnail(string familyId, int size, string filename) {
        Family family = _families[familyId];
        string fileId = Path.GetFileNameWithoutExtension(filename);
        QueryPhoto photo = _libraryProvider.GetPhoto(family, fileId);

        Thumbnail thumb = new Thumbnail(family, photo, size);

        return File(thumb.Contents, thumb.MimeType);
    }

    [Route("/Photos/{familyId}/FullSize/{filename}")]
    public IActionResult FullSize(string familyId, string filename) {
        Family family = _families[familyId];
        string fileId = Path.GetFileNameWithoutExtension(filename);
        QueryPhoto photo = _libraryProvider.GetPhoto(family, fileId);
        PhotoReader contents = new PhotoReader(family, photo);

        return File(contents.Contents, contents.MimeType);
    }

    [Route("/Photos/{familyId}/Viewer/{filename}")]
    public IActionResult Viewer(string familyId, string filename) {
        Family family = _families[familyId];
        string fileId = Path.GetFileNameWithoutExtension(filename),
            prevPhotoUrl = null,
            nextPhotoUrl = null;
        QueryPhoto photo = _libraryProvider.GetPhoto(family, fileId);
        List<QueryPhoto> allPhotos = _libraryProvider.GetPhotos(family, $"{photo.DateTaken.Month}/%/{photo.DateTaken.Year}%");

        for (int i = 0; i < allPhotos.Count; i++) {
            if (allPhotos[i].Id == fileId) {
                if (i > 0)
                    prevPhotoUrl = makeViewerUrl(family.Id, allPhotos[i - 1]);
                if (i < allPhotos.Count - 1)
                    nextPhotoUrl = makeViewerUrl(family.Id, allPhotos[i + 1]);
            }
        }

        return View(new Photos_Viewer_AspModel(new object()) {
            PhotoUrl = makePhotoUrl(family.Id, photo),
            PrevPhotoUrl = prevPhotoUrl,
            NextPhotoUrl = nextPhotoUrl
        });
    }

    string makePhotoUrl(string familyId, QueryPhoto photo)
        => $"/Photos/{familyId}/FullSize/{photo.Id}{photo.Extension}";  

    string makeViewerUrl(string familyId, QueryPhoto photo)
        => $"/Photos/{familyId}/Viewer/{photo.Id}{photo.Extension}";  
}
