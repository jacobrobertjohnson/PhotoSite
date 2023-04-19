using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Authentication;

namespace PhotoSite.Controllers;

[Authorize]
public class PhotosController : _BaseController
{
    string[] _photosFamilies;

    public PhotosController(IServiceProvider dependencies) : base(dependencies) {
        IAuthenticator authenticator = dependencies.GetService<IAuthenticator>();
        _photosFamilies = authenticator.GetClaimValue("photosFamilies").Split(',');
    }

    [Route("/Photos/{familyId}")]
    public IActionResult Index(string familyId)
    {
        if (!_photosFamilies.Contains(familyId))
            return RedirectToAction("Index", "Home");

        return View();
    }
}
