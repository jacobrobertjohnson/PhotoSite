using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Models;
using PhotoSite.Users;
using Newtonsoft.Json;

namespace PhotoSite.Controllers;

[AllowAnonymous]
public class LoginController : _BaseController
{
    IUserProvider _userProvider;

    public LoginController(IServiceProvider dependencies) : base(dependencies) {
        _userProvider = dependencies.GetService<IUserProvider>();
    }

    public IActionResult Index(bool failed = false)
    {
        var model = new Login_Index_Model();

        if (failed)
        {
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
        AuthenticatedUser user = await _userProvider.Authenticate(form.Username, form.Password);

        if (user != null) {
            var claims = new List<Claim>() {
                new Claim("user", user.Username),
                new Claim("role", "user"),
                new Claim("userId", user.UserId.ToString()),
                new Claim("userAdmin", user.UserAdmin.ToString()),
                new Claim("families", JsonConvert.SerializeObject(user.Families)),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role"));

            await HttpContext.SignInAsync(principal);

            response = RedirectToAction("Index", "Home");
        }

        return response;
    }

    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Index");
    }
}
