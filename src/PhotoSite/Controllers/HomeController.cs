using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Models;

namespace PhotoSite.Controllers;

[Authorize]
public class HomeController : _BaseController
{
    public HomeController(IServiceProvider dependencies) : base(dependencies) { }

    public IActionResult Index()
    {
        return View(new Home_Index_Model(_dependencies));
    }
}
