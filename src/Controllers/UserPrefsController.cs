using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoSite.Authentication;
using PhotoSite.Crypto;
using PhotoSite.Models;
using PhotoSite.Users;

namespace PhotoSite.Controllers;

[Authorize]
public class UserPrefsController : _BaseController {
    IUserProvider _userProvider;
    ICryptoProvider _cryptoProvider;
    string _username,
        _machineKey;

    public UserPrefsController(IServiceProvider dependencies) : base(dependencies) {
        var authenticator = dependencies.GetService<IAuthenticator>();

        _username = authenticator.GetClaimValue("user");

        _userProvider = dependencies.GetService<IUserProvider>();
        _authenticator = dependencies.GetService<IAuthenticator>();
        _cryptoProvider = dependencies.GetService<ICryptoProvider>();
        _machineKey = dependencies.GetService<AppSettings>().MachineKey;
    }

    public IActionResult Password(string result) {
        return View(new UserPrefs_Password_Model(result));
    }

    public async Task<IActionResult> ChangePassword([FromForm] UserPrefs_ChangePassword_Request form) {
        string result = "success";

        if (string.IsNullOrWhiteSpace(form.CurrentPassword))
            result = "currentPasswordRequired";
        else if (string.IsNullOrWhiteSpace(form.NewPassword))
            result = "newPasswordRequired";
        else if (string.IsNullOrWhiteSpace(form.ConfirmNewPassword))
            result = "confirmPasswordRequired";
        else if (form.ConfirmNewPassword != form.NewPassword)
            result = "mismatch";

        if (result == "success") {
            string username = _authenticator.GetClaimValue("user");

            if ((await _userProvider.Authenticate(username, form.CurrentPassword)) == null)
                result = "currentPasswordWrong";
            else {
                string hashedNewPassword = _cryptoProvider.HashValue(form.NewPassword, username, _machineKey);

                await _userProvider.SetPassword(_userId, hashedNewPassword);
            }
        }

        return RedirectToAction("Password", new {
            result
        });
    }
}