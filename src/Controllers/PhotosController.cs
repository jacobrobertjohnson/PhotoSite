using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Authentication;
using PhotoSite.Library;
using PhotoSite.Models;

namespace PhotoSite.Controllers;

[Authorize]
public class PhotosController : _BaseController
{
    ILibraryProvider _libraryProvider;
    string[] _photosFamilies;

    public PhotosController(IServiceProvider dependencies) : base(dependencies) {
        IAuthenticator authenticator = dependencies.GetService<IAuthenticator>();
        _photosFamilies = authenticator.GetClaimValue("photosFamilies").Split(',');

        _libraryProvider = dependencies.GetService<ILibraryProvider>();
    }

    [Route("/Photos/{familyId}")]
    public IActionResult Index(string familyId)
    {
        IActionResult response = RedirectToAction("Index", "Home");

        if (_photosFamilies.Contains(familyId))
            response = View(new ModelBase(new Photos_Index_VueModel() {
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
}
