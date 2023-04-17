using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Models;

namespace PhotoSite.Controllers;

[AllowAnonymous]
public class LoginController : Controller
{
    public IActionResult Index(bool failed = false)
    {
        var model = new Login_Index_Model();

        if (failed) {
            model.Message = "Invalid username/password";
            model.MessageType = "failed";
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate(Login_Authenticate_Request form) {
        IActionResult response = RedirectToAction("Index", new {
            Failed = true
        });

        if (form.Username?.ToLower() == "jake") {
            var claims = new List<Claim>() {
                new Claim("user", "jake"),
                new Claim("role", "admin"),
                new Claim("families", "Salo,JohnsonJake,JohnsonJeff")
            };

            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));

            response = RedirectToAction("Index", "Home");
        }

        return response;
    }
}
