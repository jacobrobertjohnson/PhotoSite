using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Authentication;
using PhotoSite.Crypto;
using PhotoSite.Models;
using PhotoSite.Users;

namespace PhotoSite.Controllers;

[Authorize]
public class UsersController : _BaseController {
    IUserProvider _userProvider;
    ICryptoProvider _cryptoProvider;

    Dictionary<string, Family> _families;
    string _machineKey;

    public UsersController(IServiceProvider dependencies) : base(dependencies) {
        AppSettings config = dependencies.GetService<AppSettings>();

        _machineKey = config.MachineKey;

        _families = dependencies.GetService<AppSettings>()
            .Families
            .ToDictionary(
                fam => fam.Id,
                fam => fam
            );

        _userProvider = dependencies.GetService<IUserProvider>();
        _cryptoProvider = dependencies.GetService<ICryptoProvider>();
    }

    public async Task<IActionResult> Index(string result)
        => View(new Users_Index_Model() {
            Users = await _userProvider.GetAllUsers(),
        });

    [HttpGet]
    [Route("[controller]/{userId}")]
    public async Task<IActionResult> GetUser(int userId)
    {
        var user = await _userProvider.GetUser(userId) ?? new();

        foreach (var family in _families)
            if (!user.Permissions.ContainsKey(family.Key))
                user.Permissions[family.Key] = new();

        return View(user);
    }

    [HttpPost]
    [Route("[controller]/{userId}")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] Users_User_Model body) {
        string action = userId > 0 ? "updated." : "added.";
        string message = null;

        if (string.IsNullOrEmpty(body.Username))
            message = "A username is required.";

        if (userId == 0 && string.IsNullOrEmpty(body.NewPassword))
            message = "A password is required for new users.";

        if (message == null) {
            if (userId == 0)
                userId = await _userProvider.AddUser(body);
            else {
                await _userProvider.UpdateUser(userId, body);

                string newPassword  = body.NewPassword?.Trim();

                if (!string.IsNullOrEmpty(newPassword)) {
                    string hashedPassword = _cryptoProvider.HashValue(newPassword, body.Username, _machineKey);
                    await _userProvider.SetPassword(userId, hashedPassword);
                }
            }

            foreach (var family in body.Permissions) {
                await _userProvider.SetUserFamilyPermissions(userId, family.Key, family.Value);
            }
        }

        return Ok(new {
            Success = message == null,
            Message = message ?? $"The user was successfully {action}",
        });
    }
}